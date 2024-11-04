using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using static VirtualizedGrid.Protos.PartService;
using VirtualizedGrid.Protos;
using VirtualizedGrid.Server;
using Models = VirtualizedGrid.Server.Models; // Alias for DbContext Part model

// Aliases to avoid conflicts with the generated proto classes
using ProtoPart = VirtualizedGrid.Protos.Part;
using ProtoPartStatus = VirtualizedGrid.Protos.PartStatus;
using System.Collections.Generic;

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

    public override async Task GetParts(PartsRequest request, IServerStreamWriter<PartsBatch> responseStream, ServerCallContext context)
    {
        int chunkSize = request.ChunkSize > 0 ? request.ChunkSize : 1000;

        // Check if data is already cached
        if (!_cache.TryGetValue(CacheKey, out List<Models.Part> cachedParts))
        {
            // Load all parts from the database once
            cachedParts = await _context.GetAllPartsAsync();

            // Cache the entire parts list with an expiration time (e.g., 1 hour)
            _cache.Set(CacheKey, cachedParts, TimeSpan.FromHours(1));
        }

        // Stream data in chunks from the cached parts
        foreach (var batch in cachedParts.Chunk(chunkSize))
        {
            var partsBatch = new PartsBatch
            {
                Parts = { batch.Select(p => MapToProtoPart(p)) }
            };

            await responseStream.WriteAsync(partsBatch);
        }
    }

    // Helper method to map DbContext Part to Proto Part
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
}
