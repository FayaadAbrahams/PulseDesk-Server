using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseDesk.Data;
using PulseDesk.DTOs.Comments;
using PulseDesk.Models;

namespace PulseDesk.Controllers
{
    [ApiController]
    [Route("api/tickets/{ticketId}/comments")]
    [Authorize]
    public class CommentsController(AppDbContext db) : BaseController
    {
        private readonly AppDbContext _db = db;

        // GET api/tickets/{ticketId}/comments
        [HttpGet]
        public async Task<IActionResult> GetAll(int ticketId)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId);

            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found" });
            }

            var comments = await _db.Comments.Include(c => c.User).Where(c => c.TicketId == ticketId).OrderBy(c => c.CreatedAt).Select(c => new CommentResponse
            {
                Id = c.Id,
                Body = c.Body,
                AuthorName = c.User.FullName,
                CreatedAt = c.CreatedAt
            }).ToListAsync();

            return Ok(comments);

        }

        // POST api/tickets/{ticketId}/comments
        [HttpPost]
        public async Task<IActionResult> Create(int ticketId, [FromBody] CreateCommentRequest req)
        {
            var ticket = await _db.Tickets.FindAsync(ticketId);

            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found" });
            }

            var comment = new Comment
            {
                TicketId = ticketId,
                Body = req.Body,
                UserId = CurrentUserId
            };

            await _db.Comments.AddAsync(comment);
            await _db.SaveChangesAsync();

            await _db.Entry(comment).Reference(c => c.User).LoadAsync();

            return CreatedAtAction(nameof(GetAll), new
            {
                ticketId
            },
                new CommentResponse
                {
                    Id = comment.Id,
                    Body = comment.Body,
                    AuthorName = comment.User.FullName,
                    CreatedAt = comment.CreatedAt
                });
        }

        // DELETE api/tickets/{ticketId}/comments/{commentId}
        [HttpDelete]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(int ticketId, int id)
        {
            var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);

            if (comment == null) return NotFound(new { message = "Comment not found" });

            _db.Comments.Remove(comment);

            await _db.SaveChangesAsync();

            return Ok(new { message = "Comment deleted successfully" });
        }
    }
}
