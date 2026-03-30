using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseDesk.Data;
using PulseDesk.Models.Enums;

namespace PulseDesk.Controllers
{
    /// <summary>
    /// Dashboard controller provides endpoints for statistics for PulseDesk. Only accessible through Admins and Agents (Privacy amongst costumers)
    /// </summary>
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

        /// <summary>
        /// Fetch the workload of each agent. 
        /// Helpful to know if there's backlogs.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/Dashboard/stats
        /// 
        /// </remarks>
        /// <returns>A agentWorkload Response - consists of all agents, any open tickets, in progress, resolve and total assigned tickets</returns>
        /// <response code="200">Agent work load was pulled successfully</response>
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

        /// <summary>
        /// Get the most recent activity done on the system. This includes recent tickets created and recent updates on tickets
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET api/dashboard/recent-activity
        /// 
        /// </remarks>
        /// <returns>A success response</returns>
        /// <response code="200">Allows the user to view the most recent tickets/activity</response>
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

        /// <summary>
        /// Fetch a list of all users in the system, their role and how many tickets they have raised or assigned to them. This is useful for Admins to manage users and see the workload of each user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/Dashboard/users
        /// 
        /// </remarks>
        /// <returns>A success response</returns>
        /// <response code="200">Fetched the users in the system</response>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.Users.Select(i => new
            {
                i.Id,
                i.FullName,
                i.Email,
                Role = i.Role.ToString(),
                i.IsActive,
                i.CreatedAt,
                TotalTicketsRaised = i.RaisedTickets.Count(),
                TotalTicketsAssigned = i.AssignedTickets.Count()
            }).OrderBy(u => u.Role).ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Deletes a user in the system. Only Admins can delete users.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE /api/Dashboard/delete-user/1
        /// 
        /// </remarks>
        /// <returns>A success response</returns>
        /// <response code="200">Deleted the user successfully</response>
        /// <response code="404">No User found that matches that specific id</response>
        [HttpDelete("delete-user/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id
            )
        {
            var user = await _db.Users.FirstOrDefaultAsync(c => c.Id == id && c.Id == id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            await _db.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
            await _db.SaveChangesAsync();

            return Ok(new { message = $"User {user.Email} deleted successfully" });

        }
    }
}
