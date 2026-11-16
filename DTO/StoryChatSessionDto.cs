namespace TicketTitan.DTO
{
    public class StoryChatSessionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<StoryChatMessageDto> Messages { get; set; } = new();
    }
}
