using PulseDesk.Models.Enums;

namespace PulseDesk.DTOs.Tickets
{
    public class UpdateTicketRequest
    {
        public StatusType? Status { get; set; }
        public PriorityType? Priority { get; set; }
        public int? AgentId { get; set; }
    }
}
