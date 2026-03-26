namespace PulseDesk.DTOs.Tickets
{
    public class TicketResponse
    {

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string? AgentName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}
