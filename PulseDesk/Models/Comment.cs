namespace PulseDesk.Models
{
    public class Comment
    {
        public required int Id { get; set; }
        public required int TicketId { get; set; }
        public required int UserId { get; set; }
        public required string Body { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
