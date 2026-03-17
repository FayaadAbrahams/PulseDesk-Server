using Microsoft.EntityFrameworkCore;
using PulseDesk.Models;

namespace PulseDesk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelB)
        {
            modelB.Entity<User>()
                .HasOne(t => t.Role)
                .hasMany(i => i.Users)
        }
    }
}
