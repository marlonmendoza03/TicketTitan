using TicketTitan.Common;
using TicketTitan.DTO;

namespace TicketTitan.Interfaces
{
    public interface IGeminiChatService
    {
        Task<ServiceResult<GeminiChatResponseDto>> SendChatAsync(string prompt, string userId, string model, CancellationToken cancellationToken = default);
        Task<ServiceResult<GeminiChatResponseDto>> SendChatAsync(IReadOnlyList<(string Role, string Content)> messages, string model, CancellationToken cancellationToken = default);
    }
}
