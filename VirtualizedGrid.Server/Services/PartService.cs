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

    public override async Task GetParts(PartsRequest request, IServerStreamWriter<PartsBatch> responseStream, ServerCallContext context)
    {
        try
        {
            int chunkSize = request.ChunkSize > 0 ? request.ChunkSize : 1000;
            Guid? lastFetchedId = null;

            while (true)
            {
                // Use the stored procedure to get parts in chunks
                var parts = await _context.GetPartsInChunksAsync(lastFetchedId, chunkSize);

                if (parts.Count == 0) break;

                // Create a PartsBatch and add the retrieved parts
                var batch = new PartsBatch
                {
                    Parts =
                    {
                        parts.Select(p => new Part
                        {
                            Id = p.Id.ToString(),
                            Name = p.Name,
                            CreationDate = p.CreationDate.ToString("o"),
                            Status = (PartStatus)p.Status
                        })
                    }
                };

                // Send the entire batch in one WriteAsync call
                await responseStream.WriteAsync(batch);

                // Update the last fetched ID for the next iteration
                lastFetchedId = parts.Last().Id;
            }
        }
        catch (Exception ex)
        {
            // Handle exception logging if necessary
        }
    }
}
