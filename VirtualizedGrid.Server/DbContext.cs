using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VirtualizedGrid.Server.Models;

namespace VirtualizedGrid.Server
{
    public class AppDbContext : DbContext
    {
        public DbSet<Part> Parts { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public async Task<List<Part>> GetPartsInChunksAsync(Guid? lastFetchedId, int chunkSize)
        {
            var lastFetchedIdParam = new SqlParameter("@LastFetchedId", lastFetchedId ?? (object)DBNull.Value);
            var chunkSizeParam = new SqlParameter("@ChunkSize", chunkSize);

            return await Parts
                .FromSqlRaw("EXEC GetPartsInChunks @LastFetchedId, @ChunkSize", lastFetchedIdParam, chunkSizeParam)
                .AsNoTracking()
                .ToListAsync();
        }

    }
}
