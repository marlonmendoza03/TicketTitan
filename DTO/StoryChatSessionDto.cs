using TicketTitan.Models;

namespace TicketTitan.DTO
{
    public class StoryChatSessionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public SessionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int MessageCount { get; set; }
        public List<StoryAttachmentDto> Attachments { get; set; } = new();
        public List<StoryChatMessageDto> Messages { get; set; } = new();
    }
}
