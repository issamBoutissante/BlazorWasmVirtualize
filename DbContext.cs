using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using VirtualizedGrid.Server.Models;

namespace VirtualizedGrid.Server
{
    public class AppDbContext : DbContext
    {
        public DbSet<Part> Parts { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }

}
