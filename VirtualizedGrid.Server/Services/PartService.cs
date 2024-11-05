using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using static VirtualizedGrid.Protos.PartService;
using VirtualizedGrid.Protos;
using VirtualizedGrid.Server;
using Models = VirtualizedGrid.Server.Models;
using Google.Protobuf;
using System.IO.Compression;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using ProtoPart = VirtualizedGrid.Protos.Part;
using ProtoPartStatus = VirtualizedGrid.Protos.PartStatus;

namespace VirtualizedGrid.Server.Services;

public class PartService : PartServiceBase
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly string CacheKey = "PartsDataCache";

    public PartService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public override async Task GetParts(PartsRequest request, IServerStreamWriter<CompressedPartsBatch> responseStream, ServerCallContext context)
    {
        int chunkSize = request.ChunkSize > 0 ? request.ChunkSize : 1000;

        // Retrieve data from cache or database
        if (!_cache.TryGetValue(CacheKey, out List<Models.Part> cachedParts))
        {
            cachedParts = await _context.GetAllPartsAsync();
            _cache.Set(CacheKey, cachedParts, TimeSpan.FromHours(1));
        }

        // Stream data in compressed chunks
        foreach (var batch in cachedParts.Chunk(chunkSize))
        {
            // Create the PartsBatch and add Parts individually
            var partsBatch = new PartsBatch();
            partsBatch.Parts.AddRange(batch.Select(p => MapToProtoPart(p)));

            // Now continue with the compression
            var compressedData = CompressData(partsBatch.ToByteArray());
            var compressedBatch = new CompressedPartsBatch { Data = ByteString.CopyFrom(compressedData) };
            await responseStream.WriteAsync(compressedBatch);
        }
    }

    public override async Task<PartsCountResponse> GetPartsCount(Empty request, ServerCallContext context)
    {
        int totalCount = await _context.Parts.CountAsync();
        return new PartsCountResponse { TotalCount = totalCount };
    }

    private ProtoPart MapToProtoPart(Models.Part dbPart)
    {
        return new ProtoPart
        {
            Id = dbPart.Id.ToString(),
            Name = dbPart.Name,
            CreationDate = dbPart.CreationDate.ToString("o"),
            Status = (ProtoPartStatus)dbPart.Status
        };
    }

    private byte[] CompressData(byte[] data)
    {
        using (var output = new MemoryStream())
        using (var gzip = new GZipStream(output, CompressionMode.Compress))
        {
            gzip.Write(data, 0, data.Length);
            gzip.Close();
            return output.ToArray();
        }
    }
}

// PartsBatch class to hold Parts and serialize to byte array
public class PartsBatch
{
    public List<ProtoPart> Parts { get; set; } = new List<ProtoPart>();

    public byte[] ToByteArray()
    {
        var batchMessage = new CompressedPartsBatch();
        var protoBatch = new VirtualizedGrid.Protos.PartsBatch
        {
            Parts = { Parts }
        };

        return protoBatch.ToByteArray();
    }
}
