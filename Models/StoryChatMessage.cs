using System.ComponentModel.DataAnnotations;

namespace TicketTitan.Models
{
    public enum RoleType { 
        User, 
        Assistant
    }

    public static class RoleTypeExtensions
    {
        public static string ToGeminiRole(this RoleType role)
        {
            return role switch
            {
                RoleType.User => "user",
                RoleType.Assistant => "assistant",
                _ => "user"
            };
        }
    }

    public class StoryChatMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public StoryChatSession Session { get; set; } = null!;
        public RoleType Role { get; set; } = RoleType.User;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
