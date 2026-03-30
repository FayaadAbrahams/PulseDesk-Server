using PulseDesk.Models.Enums;

namespace PulseDesk.DTOs.Tickets
{
    public class TicketResponse
    {

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public StatusType Status { get; set; } = StatusType.Open;

        public PriorityType Priority { get; set; } = PriorityType.Medium;

        public string CustomerName { get; set; } = string.Empty;

        public string? AgentName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}
