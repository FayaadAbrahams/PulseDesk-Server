using Microsoft.EntityFrameworkCore;
using PulseDesk.Models;
using PulseDesk.Models.Enums;

namespace PulseDesk.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelB)
        {
            base.OnModelCreating(modelB);

            modelB.Entity<User>(entity =>
            {
                entity.HasIndex(i => i.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>().HasDefaultValue(UserRole.Customer);
                entity.Property(i => i.IsActive).HasDefaultValue(true);
            });

            modelB.Entity<Ticket>(entity =>
            {
                entity.Property(i => i.Status).HasDefaultValue(StatusType.Open);
                entity.Property(u => u.Priority).HasConversion<string>().HasDefaultValue(PriorityType.Low);
                entity.HasOne(i => i.Customer).WithMany(u => u.RaisedTickets).HasForeignKey(t => t.CustomerId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(i => i.Agent).WithMany(u => u.AssignedTickets).HasForeignKey(t => t.AgentId).OnDelete(DeleteBehavior.Restrict);
            });

            modelB.Entity<Comment>(entity =>
            {
                entity.HasOne(i => i.Ticket).WithMany(u => u.Comments).HasForeignKey(t => t.TicketId).OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.User).WithMany(u => u.Comments).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelB.Entity<AuditLog>(entity =>
            {
                entity.HasOne(a => a.Ticket).WithMany(t => t.AuditLogs).HasForeignKey(a => a.TicketId);
                entity.HasOne(a => a.ChangedBy).WithMany(u => u.AuditLogs).HasForeignKey(a => a.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
