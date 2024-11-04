using VirtualizedGrid.Server.Models;

namespace VirtualizedGrid.Server.DTOs
{
    public class PartDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }=string.Empty;
        public DateTimeOffset CreationDate { get; set; }
        public PartStatus Status { get; set; }
    }
}
