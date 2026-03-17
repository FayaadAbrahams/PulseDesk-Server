namespace PulseDesk.Models
{
    public class AuditLog
    {
        public required int Id { get; set; }
        
        public required int TicketId { get; set; }

        public required int ChangedByUserId { get; set; }

        public required string Field{ get; set; }

        public required string OldValue{ get; set; }

        public required string NewValue{ get; set; }

        public required DateTime ChangedAt { get; set; }

    }
}
