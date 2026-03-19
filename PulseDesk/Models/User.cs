namespace PulseDesk.Models;

using PulseDesk.Models.Enums;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int Id { get; set; }

    [MaxLength(150)]
    public  string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public  string Email { get; set; } = string.Empty;

    [Required]
    public  string PasswordHash { get; set; } = string.Empty;

    [Required]
    public  UserRole Role { get; set; } = UserRole.Customer;
    
    [Required]
    public  DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // New field to track if the user is active or not - Added for soft deletes and account management,
    // allows us to deactivate accounts without permanently deleting them
    public bool IsActive { get; set; } = true;

    public ICollection<Ticket> RaisedTickets { get; set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();


}
