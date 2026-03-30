namespace PulseDesk.DTOs.Comments
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public string Body { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
