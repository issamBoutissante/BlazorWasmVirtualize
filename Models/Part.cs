namespace VirtualizedGrid.Server.Models
{
    public class Part
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTimeOffset CreationDate { get; set; }
        public PartStatus Status { get; set; }
    }
    public enum PartStatus
    {
        Available,
        OutOfStock,
        Discontinued,
        BackOrder
    }

}
