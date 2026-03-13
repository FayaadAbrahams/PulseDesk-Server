namespace PulseDesk.Models;

using PulseDesk.Models.Enums;

public class Ticket
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string  Description { get; set; }
    public StatusType Status { get; set; }
    
    public required string Priority { get; set; }

    public required int CustomerId { get; set; }

    public int? AgentId { get; set; }

    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }

}

