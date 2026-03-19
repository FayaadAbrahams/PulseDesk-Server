using System.ComponentModel.DataAnnotations;

namespace PulseDesk.DTOs.Tickets
{
    public class CreateTicketRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = "Medium";
    }
}
