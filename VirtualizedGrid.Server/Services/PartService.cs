using VirtualizedGrid.Protos;
using Microsoft.EntityFrameworkCore;
using Grpc.Core;

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
            int skip = 0;
            int chunkSize = request.ChunkSize > 0 ? request.ChunkSize : 100;

            while (true)
            {
                var parts = await _context.Parts
                    .Skip(skip)
                    .Take(chunkSize)
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

                skip += chunkSize;
                if (skip == 1000)
                    break;
            }
        }catch (Exception ex)
        {

        }
    }
}