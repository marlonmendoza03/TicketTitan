namespace TicketTitan.DTO
{
    public class StoryChatSessionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int MessageCount { get; set; }
        public List<StoryChatMessageDto> Messages { get; set; } = new();
    }
}
