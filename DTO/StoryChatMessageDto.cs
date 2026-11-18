using TicketTitan.Models;

namespace TicketTitan.DTO
{
    public class StoryChatMessageDto
    {
        public RoleType? Role { get; set; } = null;
        public string Content { get; set; } = string.Empty;
        public StorySetup? StorySetup { get; set; }
    }
}
