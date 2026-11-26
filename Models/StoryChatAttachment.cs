using System.ComponentModel.DataAnnotations;

namespace TicketTitan.Models
{
    public class StoryChatAttachment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public StoryChatSession Session { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public AttachmentType Type { get; set; } = AttachmentType.Character;
        public string FileName { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
