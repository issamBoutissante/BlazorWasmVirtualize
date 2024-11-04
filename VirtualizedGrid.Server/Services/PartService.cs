using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using static VirtualizedGrid.Protos.PartService;
using VirtualizedGrid.Protos;
using VirtualizedGrid.Server;
using Models=VirtualizedGrid.Server.Models; // Namespace for the model Part

// Alias to avoid conflict with the DbContext Part
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
        Guid? lastFetchedId = null;

        // Check if data is already cached
        if (!_cache.TryGetValue(CacheKey, out List<Models.Part> cachedParts))
        {
            cachedParts = new List<Models.Part>();

            while (true)
            {
                // Fetch data from the database in chunks
                var parts = await _context.GetPartsInChunksAsync(lastFetchedId, chunkSize);
                if (parts.Count == 0) break;

                cachedParts.AddRange(parts);

                // Update last fetched ID
                lastFetchedId = parts.Last().Id;
            }

            // Cache the parts data with an expiration time (e.g., 1 hour)
            _cache.Set(CacheKey, cachedParts, TimeSpan.FromHours(1));
        }

        // Send data in batches from cache
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
