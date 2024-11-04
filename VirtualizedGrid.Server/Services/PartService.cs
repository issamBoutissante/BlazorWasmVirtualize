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
            int chunkSize = request.ChunkSize > 0 ? request.ChunkSize : 1000;
            Guid? lastFetchedId = null;

            while (true)
            {
                var parts = await _context.GetPartsInChunksAsync(lastFetchedId, chunkSize);

                if (parts.Count == 0) break;

                foreach (var part in parts)
                {
                    await responseStream.WriteAsync(new Part
                    {
                        Id = part.Id.ToString(),
                        Name = part.Name,
                        CreationDate = part.CreationDate.ToString("o"),
                        Status = (PartStatus)part.Status
                    });
                }

                lastFetchedId = parts.Last().Id;
            }
        }
        catch (Exception ex)
        {
            // Handle exception logging if necessary
        }
    }

}
