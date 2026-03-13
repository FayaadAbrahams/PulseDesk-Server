namespace PulseDesk.Models;

using PulseDesk.Models.Enums;

public class User
{
    public required int Id { get; set; }
    
  public required string FullName { get; set; }
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public required UserRole Role { get; set; }

    public required DateTime CreatedAt { get; set; }

    public required Boolean IsActive { get; set; }

}
