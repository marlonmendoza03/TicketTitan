namespace TicketTitan.DTO
{
    public class GeminiChatRequestDto
    {
        public List<GeminiChatMessageDto> Messages { get; set; }
        public string Model { get; set; } = "gemini-3.5-flash-lite";
        public StorySetup StorySetup { get; set; }
    }
}
