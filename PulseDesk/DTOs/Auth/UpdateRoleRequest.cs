using PulseDesk.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PulseDesk.DTOs.Auth
{
    public class UpdateRoleRequest
    {
        [Required]
        public UserRole Role { get; set; }


    }
}
