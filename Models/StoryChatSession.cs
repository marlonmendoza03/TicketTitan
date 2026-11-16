using System.ComponentModel.DataAnnotations;

namespace TicketTitan.Models
{
    public class StoryChatSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<StoryChatMessage> Messages { get; set; } = new List<StoryChatMessage>();
    }
}
