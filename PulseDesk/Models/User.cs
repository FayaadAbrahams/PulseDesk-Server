namespace PulseDesk.Models;

using PulseDesk.Models.Enums;

public class User
{
    public required string Id { get; set; }
    public required UserRole RoleName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public DateTime? BirthDate { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Password { get; set; }

}
