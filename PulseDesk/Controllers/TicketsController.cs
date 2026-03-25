using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseDesk.Data;
using PulseDesk.DTOs.Tickets;
using PulseDesk.Models;
using PulseDesk.Models.Enums;
using System.Security.Claims;

namespace PulseDesk.Controllers
{
    // [Authorize] Is for JWT Authorization, it ensures that only authenticated users can access the endpoints in this controller
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController(AppDbContext db) : ControllerBase
    {

        private readonly AppDbContext _db = db;
        [HttpGet]

        // Get api/tickets
        // Only customers can see their own tickets
        // Agents & admins can see all the tickets  
        public async Task<IActionResult> GetAll()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            var query = _db.Tickets.Include(t => t.Customer).Include(t => t.Agent).AsQueryable();

            // Only customers can see their own tickets
            if (role == UserRole.Customer.ToString())
            {
                query = query.Where(t => t.CustomerId == userId);
            }

            var tickets = await query.OrderByDescending(t => t.CreatedAt).Select(t => new TicketResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                CustomerName = t.Customer.FullName,
                AgentName = t.Agent != null ? t.Agent.FullName : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToListAsync();

            return Ok(tickets);
        }

        [HttpGet("debug-claims")]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)!.Value;
            var ticket = await _db.Tickets.Include(t => t.Customer).Include(t => t.Agent).Include(t => t.Comments).ThenInclude(u => u.User).FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found" });
            }

            // Prevent the customer from accessing other customers tickets
            if (role == UserRole.Customer.ToString() && ticket.CustomerId != userId)
            {
                return Forbid();
            }

            return Ok(new TicketResponse
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status.ToString(),
                Priority = ticket.Priority,
                CustomerName = ticket.Customer.FullName,
                AgentName = ticket.Agent?.FullName,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt
            });

        }

        // Only Customers can create tickets
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequest req)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var ticket = new Ticket
            {
                Title = req.Title,
                Description = req.Description,
                Priority = req.Priority.ToString(),
                CustomerId = userId,
                Status = StatusType.Open
            };

            await _db.Tickets.AddAsync(ticket);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ticket.Id },
                new { message = "Ticket created successfully", ticketId = ticket.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Agent,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketRequest req)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var ticket = await _db.Tickets.FindAsync(id);

            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found" });
            }

            if (req.Status != null && req.Status != ticket.Status.ToString())
            {
                await LogChange(ticket.Id, userId, "Status", ticket.Status.ToString(), req.Status);
            }

            if (req.Priority != null && req.Priority != ticket.Priority)
            {
                await LogChange(ticket.Id, userId, "Priority", ticket.Priority, req.Priority);
                ticket.Priority = req.Priority;
            }

            if (req.AgentId != null && req.AgentId != ticket.AgentId)
            {
                await LogChange(ticket.Id, userId, "AgentId", ticket.AgentId?.ToString() ?? "Unassigned", req.AgentId.ToString()!);
                ticket.AgentId = req.AgentId;
            }

            ticket.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Ticket updated successfully" });
        }

        // Delete api/tickets/{id}

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);

            if(ticket == null)
            {
                return NotFound(new {message = "Ticket not found" });
            }

            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Ticket deleted successfully" });
        }

        // Keeping a paper trail of whatever happens with a ticket 
        private async Task LogChange(int ticketId, int userId, string field, string oldValue, string newValue)
        {
            var log = new AuditLog
            {
                TicketId = ticketId,
                ChangedByUserId = userId,
                Field = field,
                OldValue = oldValue,
                NewValue = newValue
            };

            await _db.AuditLogs.AddAsync(log);
        }


    }


}
