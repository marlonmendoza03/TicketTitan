using TicketTitan.Common;
using TicketTitan.DTO;

namespace TicketTitan.Interfaces
{
    public interface IStoryChatService
    {
        Task<ServiceResult<StoryChatSessionDto>> CreateSessionAsync(string userId, string? title);
        Task<List<StoryChatSessionDto>> GetSessionsAsync(string userId);
        Task<ServiceResult<StoryChatSessionDto?>> GetSessionAsync(Guid sessionId, string userId);
        Task<ServiceResult<StoryChatSessionDto>> SendMessageAsync(Guid sessionId, string userId, string content, string model = "gemini-3.5-flash-lite");
        Task<ServiceResult<StoryChatSessionDto>> StartChatAsync(string userId, string initialPrompt, StorySetup storySetup, string model = "gemini-3.5-flash-lite");
        Task<ServiceResult<StoryChatSessionDto>> ApproveSessionAsync(Guid sessionId, string userId);
        Task<ServiceResult<bool>> DeleteSessionAsync(Guid sessionId, string userId);
    }
}
