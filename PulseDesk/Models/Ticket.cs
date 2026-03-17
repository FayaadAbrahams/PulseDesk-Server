namespace PulseDesk.Models;

using PulseDesk.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Ticket
{
    [Key]
    [MaxLength(150)]
    public  int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    [Required]
    public StatusType Status { get; set; } = StatusType.Open;

    [Required]
    public string Priority { get; set; } = "Medium";

    [Required]
    public int CustomerId { get; set; }

    public int? AgentId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("CustomerId")]
    public User Customer { get; set; } = null;

    [ForeignKey("AgentId")]
    public User? Agent { get; set; }


    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();


}

