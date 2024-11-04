using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtualizedGrid.Server.DTOs;
using Models = VirtualizedGrid.Server.Models; // Alias for DbContext Part model

namespace VirtualizedGrid.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PartsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly string CacheKey = "PartsDataCache";

    public PartsController(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [HttpGet(nameof(GetAllParts))]
    public async Task<ActionResult<IEnumerable<PartDto>>> GetAllParts([FromQuery] int offset = 0, [FromQuery] int limit = 1000)
    {
        // Validate limit to avoid excessive data loads
        limit = Math.Clamp(limit, 1, 20_000); // Set maximum limit to 20,000

        // Check if data is already cached
        if (!_cache.TryGetValue(CacheKey, out List<Models.Part> cachedParts))
        {
            // Load all parts from the database once using the stored procedure
            cachedParts = await _context.GetAllPartsAsync();

            // Cache the entire parts list with an expiration time (e.g., 1 hour)
            _cache.Set(CacheKey, cachedParts, TimeSpan.FromHours(1));
        }

        // Apply chunking from cache based on offset and limit
        var chunk = cachedParts.Skip(offset).Take(limit).Select(p => new PartDto
        {
            Id = p.Id,
            Name = p.Name,
            CreationDate = p.CreationDate,
            Status = p.Status
        }).ToList();

        return Ok(chunk);
    }
    [HttpGet(nameof(GetPartsCount))]
    public async Task<ActionResult<IEnumerable<PartDto>>> GetPartsCount([FromQuery] int offset = 0, [FromQuery] int limit = 1000)
    {
        return Ok(await _context.Parts.CountAsync());
    }
}
