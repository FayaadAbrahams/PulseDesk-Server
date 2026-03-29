using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseDesk.Data;
using PulseDesk.Models.Enums;

namespace PulseDesk.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Agent")]
    public class DashboardController(AppDbContext db) : BaseController
    {
        private readonly AppDbContext _db = db;

        // GET api/dashboard/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalTickets = await _db.Tickets.CountAsync();

            var ticketsByStatus = await _db.Tickets
                .GroupBy(t => t.Status)
                .Select(g => new
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                }).ToListAsync();

            var ticketsByPriority = await _db.Tickets
                .GroupBy(t => t.Priority)
                .Select(g => new
                {
                    Priority = g.Key.ToString(),
                    Count = g.Count()
                }).ToListAsync();

            var noUserAssignTickets = await _db.Tickets.CountAsync(
                t => t.AgentId == null);

            var resolvedTickets = await _db.Tickets.CountAsync(t => t.Status == Models.Enums.StatusType.Resolved);

            return Ok(new
            {
                TotalTickets = totalTickets,
                NoAssignedTickets = noUserAssignTickets,
                ResolvedTickets = resolvedTickets,
                TicketsByStatus = ticketsByStatus,
                TicketsPriority = ticketsByPriority
            });
        }

        // GET api/dashboard/agent-workload 
        [HttpGet("agent-workload")]
        public async Task<IActionResult> GetAgentWorkLoad()
        {
            var agentWorkload = await _db.Users.Where(u => u.Role == UserRole.Agent).Select(u => new
            {
                AgentName = u.FullName,
                OpenTickets = u.AssignedTickets
                        .Count(t => t.Status == StatusType.Open),
                InProgressTickets = u.AssignedTickets
                        .Count(t => t.Status == StatusType.InProgress),
                ResolvedTickets = u.AssignedTickets
                        .Count(t => t.Status == StatusType.Resolved),
                TotalAssigned = u.AssignedTickets.Count()
            })
                .ToListAsync();

            return Ok(agentWorkload);
        }

        // GET api/dashboard/recent-activity
        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity()
        {
            var recentTickets = await _db.Tickets.Include(t => t.Customer)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5).Select(t => new
                {
                    t.Id,
                    t.Title,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    Customer = t.Customer.FullName,
                    t.CreatedAt
                }).ToListAsync();

            var recentActivity = await _db.AuditLogs
                .Include(a => a.ChangedBy)
                .Include(a => a.Ticket)
                .OrderByDescending(a => a.ChangedAt)
                .Take(10)
                .Select(a => new
                {
                    a.Ticket.Title,
                    ChangedBy = a.ChangedBy.FullName,
                    a.FieldName,
                    a.OldValue,
                    a.NewValue,
                    a.ChangedAt
                }).ToListAsync();

            return Ok(new
            {
                RecentTickets = recentTickets,
                RecentActivity = recentActivity,
            });
        }
    }
}
