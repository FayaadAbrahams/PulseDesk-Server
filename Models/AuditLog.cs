using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PulseDesk.Models
{
    public class AuditLog
    {
        [Key]
        public  int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FieldName { get; set; } = string.Empty;

        [Required]
        public  int TicketId { get; set; }
        
        [Required]
        public int ChangedByUserId { get; set; }
        [Required]

        public string OldValue{ get; set; } = string.Empty;

        [Required]
        public string NewValue { get; set; } = string.Empty;
        
        [Required]
        public DateTime ChangedAt { get; set; }

        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; } = null!;

        [ForeignKey("ChangedByUserId")]
        public User ChangedBy { get; set; } = null!;

    }
}
