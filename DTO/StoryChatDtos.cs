namespace TicketTitan.DTO
{
    public record StorySetupDto(
        int CategoryId,
        string CategoryName,
        int EpisodeCount,
        int EpisodeDurationSec
    );

    public record ChatMessageDto(
        string Id,
        string Role,
        string Content,
        DateTime CreatedAt
    );

    public record StoryChatRequestDto(
        StorySetupDto Setup,
        List<ChatMessageDto> Messages
    );

    public record StoryChatResponseDto(ChatMessageDto Message);
}
