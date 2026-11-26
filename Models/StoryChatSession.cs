using System.ComponentModel.DataAnnotations;

namespace TicketTitan.Models
{
    public class StoryChatSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public SessionStatus Status { get; set; } = SessionStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<StoryChatMessage> Messages { get; set; } = new List<StoryChatMessage>();
        public ICollection<StoryChatAttachment> Attachments { get; set; } = new List<StoryChatAttachment>();
    }
}
