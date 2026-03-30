using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseDesk.Data;
using PulseDesk.DTOs.Comments;
using PulseDesk.DTOs.Tickets;
using PulseDesk.Models;
using PulseDesk.Models.Enums;
using System.Diagnostics;
using System.Security.Claims;

namespace PulseDesk.Controllers
{
    /// <summary>
    /// Tickets are the bread and butter for PulseDesk, tickets are the issues a customer will point out. Customers can create tickets, view their own tickets and see the status of their tickets. Agents can view all tickets, update the status, priority and assignment of a ticket. Admins have all the permissions of agents and can also delete tickets.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController(AppDbContext db) : BaseController
    {

        private readonly AppDbContext _db = db;

        /// <summary>
        /// Fetches all the tickets in the system. Customers can only see their own tickets, Agents and Admins can see all the tickets.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET api/tickets
        ///     Only customers can see their own tickets
        ///     Agents and admins can see all the tickets  
        /// 
        /// </remarks>
        /// <returns>A success response</returns>
        /// <response code="200">Allows the user to view the comments</response>
        [HttpGet]

        // Get api/tickets
 
        public async Task<IActionResult> GetAll()
        {
            var userId = CurrentUserId;
            var role = CurrentUserRole;

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
                Status = t.Status,
                Priority = t.Priority,
                CustomerName = t.Customer.FullName,
                AgentName = t.Agent != null ? t.Agent.FullName : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToListAsync();

            return Ok(tickets);
        }

        /// <summary>
        /// Fetches a ticket by it's id
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/Tickets/{id}
        /// 
        /// </remarks>
        /// <param name="id">The Ticket id </param>
        /// <returns>A Ticket response</returns>
        /// <response code="200">Allows the user to view the specific ticket</response>

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = CurrentUserId;
            var role = CurrentUserRole;
            var ticket = await _db.Tickets
                .Include(t => t.Customer)
                .Include(t => t.Agent)
                .FirstOrDefaultAsync(t => t.Id == id);

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
                Status = ticket.Status,
                Priority = ticket.Priority,
                CustomerName = ticket.Customer.FullName,
                AgentName = ticket.Agent?.FullName,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
            });

        }

        /// <summary>
        /// Only Customers can create tickets
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Tickets/{id}
        /// 
        /// </remarks>
        /// <param name="req">The Ticket Creation DTO </param>
        /// <returns>A Success response</returns>
        /// <response code="200">Ticket Created successfully</response>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequest req)
        {
            var userId = CurrentUserId;
             
            var ticket = new Ticket
            {
                Title = req.Title,
                Description = req.Description,
                Priority = req.Priority,
                CustomerId = userId,
                Status = StatusType.Open
            };

            await _db.Tickets.AddAsync(ticket);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ticket.Id },
                new { message = "Ticket created successfully", ticketId = ticket.Id });
        }

        /// <summary>
        /// Only Agents and Admins can update the ticket status, priority and assignment
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/Tickets/{id}
        /// 
        /// </remarks>
        /// <param name="req">The Ticket Updating DTO </param>
        /// <returns>A Success Response</returns>
        /// <response code="200">Ticket was updated successfully</response>
        /// <response code="401">Unauthorized to perform an update</response>
        /// <response code="404">Could not find the ticket to update</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Agent,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketRequest req)
        {
            var userId = CurrentUserId;

            var ticket = await _db.Tickets.FindAsync(id);

            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found" });
            }

            if (req.Status != null && req.Status != ticket.Status)
            {
                await LogChange(ticket.Id, userId, "Status", ticket.Status.ToString(), req.Status.ToString());
            }

            if (req.Priority != null && req.Priority != ticket.Priority)
            {
                await LogChange(ticket.Id, userId, "Priority", ticket.Priority.ToString(), req.Priority.ToString());
                ticket.Priority = (PriorityType)req.Priority;
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

        /// <summary>
        /// Only Agents and Admins can update the ticket status, priority and assignment
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE /api/Tickets/{id}
        /// 
        /// </remarks>
        /// <param name="id">The Ticket's id that will be deleted </param>
        /// <returns>A Success Response</returns>
        /// <response code="200">Ticket was updated successfully</response>
        /// <response code="404">Could not find the ticket to update</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);

            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found" });
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
                FieldName = field,
                OldValue = oldValue,
                NewValue = newValue
            };

            await _db.AuditLogs.AddAsync(log);
        }


    }


}
