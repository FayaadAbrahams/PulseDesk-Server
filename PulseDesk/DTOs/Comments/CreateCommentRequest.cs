using System.ComponentModel.DataAnnotations;

namespace PulseDesk.DTOs.Comments
{
    public class CreateCommentRequest
    {
        [Required]
        public string Body { get; set; } = string.Empty;
    }
}
