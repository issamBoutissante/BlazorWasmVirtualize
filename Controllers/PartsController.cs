using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualizedGrid.Server.DTOs;

namespace VirtualizedGrid.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PartsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PartsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartDto>>> GetAllParts()
    {
        var parts = await _context.Parts
            .Select(p => new PartDto
            {
                Id = p.Id,
                Name = p.Name,
                CreationDate = p.CreationDate,
                Status = p.Status
            })
            .Take(100_000) // Limit to the first 100 items
            .ToListAsync();

        return Ok(parts);
    }

}
