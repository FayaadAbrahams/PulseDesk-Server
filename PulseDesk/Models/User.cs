namespace PulseDesk.Models;

using PulseDesk.Models.Enums;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public  int Id { get; set; }

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

    public  bool IsActive { get; set; } = true;

}
