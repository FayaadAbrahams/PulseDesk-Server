namespace PulseDesk.Models
{
    public class Permission
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}