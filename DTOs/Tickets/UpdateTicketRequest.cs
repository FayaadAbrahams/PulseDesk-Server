namespace PulseDesk.DTOs.Tickets
{
    public class UpdateTicketRequest
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public int? AgentId { get; set; }
    }
}
