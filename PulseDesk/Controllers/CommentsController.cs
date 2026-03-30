using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PulseDesk.Data;
using PulseDesk.DTOs.Comments;
using PulseDesk.Models;

namespace PulseDesk.Controllers
{
    /// <summary>
    /// Endpoints for managing comments that are related to tickets. Users can create a comment for a ticket. Admins can delete any comment.
    /// </summary>
    [ApiController]
    [Route("api/tickets/{ticketId}/comments")]
    [Authorize]
    public class CommentsController(AppDbContext db) : BaseController
    {
        private readonly AppDbContext _db = db;

        /// <summary>
        /// Fetchs comments from a specific ticket
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET api/tickets/123/comments
        ///     
        /// </remarks>
        /// <param name="ticketId">The unique identifier for the ticket.</param>
        /// <returns>A User object.</returns>
        /// <response code="200">Returns the ticket's comments</response>
        /// <response code="401">If the user is not found/Authorized</response>
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

        /// <summary>
        /// Creates a comment for a specific ticket
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/tickets/123/comments
        ///     
        /// </remarks>
        /// <param name="ticketId">The unique identifier for the ticket.</param>
        /// <param name="req">Request coming from Client.</param>
        /// <returns>A Comment Response object.</returns>
        /// <response code="200">Returns the ticket's comments</response>
        /// <response code="401">If the user is not found/Authorized</response>
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
        /// <summary>
        /// Deletes a comment for a specific ticket
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE api/tickets/123/comments
        ///     
        /// </remarks>
        /// <param name="ticketId">The unique identifier for the ticket</param>
        /// <param name="id">The unique identifier for the comment</param>
        /// <returns>A code 200 response</returns>
        /// <response code="200">Returns the ticket's comments</response>
        /// <response code="401">If the user is not found/Authorized</response>
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
