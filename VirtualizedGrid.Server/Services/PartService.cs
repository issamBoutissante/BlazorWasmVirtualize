using Microsoft.EntityFrameworkCore;
using VirtualizedGrid.Server;
using Grpc.Core;
using VirtualizedGrid.Protos;

namespace VirtualizedGrid.Server.Services;

public class PartService : VirtualizedGrid.Protos.PartService.PartServiceBase
{
    private readonly AppDbContext _context;

    public PartService(AppDbContext context)
    {
        _context = context;
    }

    public override async Task GetParts(PartsRequest request, IServerStreamWriter<Part> responseStream, ServerCallContext context)
    {
        try
        {
            int chunkSize = request.ChunkSize > 0 ? request.ChunkSize : 1000; // Increased chunk size for fewer calls
            Guid? lastFetchedId = null;

            while (true)
            {
                var query = _context.Parts
                    .AsNoTracking()
                    .OrderBy(p => p.Id)
                    .Take(chunkSize);

                if (lastFetchedId.HasValue)
                {
                    query = query.Where(p => p.Id > lastFetchedId);
                }

                var parts = await query
                    .Select(p => new Part
                    {
                        Id = p.Id.ToString(),
                        Name = p.Name,
                        CreationDate = p.CreationDate.ToString("o"),
                        Status = (PartStatus)p.Status
                    })
                    .ToListAsync();

                if (parts.Count == 0) break;

                foreach (var part in parts)
                {
                    await responseStream.WriteAsync(part);
                }

                //if (parts.Last().Id == "targetIdFor100000Items") break; // Adjust for target count as needed
            }
        }
        catch (Exception ex)
        {
            // Handle exception logging if necessary
        }
    }
}
