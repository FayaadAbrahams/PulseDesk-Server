using Microsoft.EntityFrameworkCore;
using PulseDesk.Models;

namespace PulseDesk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
