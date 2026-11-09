namespace TicketTitan.DTO
{
    public class GeminiChatResponseDto
    {
        public string content { get; set; } = string.Empty;
        public string role { get; set; } = "assistant";
        public DateTime createdAt { get; set; }
    }
}
