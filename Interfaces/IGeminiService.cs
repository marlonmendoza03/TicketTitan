using TicketTitan.Common;
using TicketTitan.DTO;

namespace TicketTitan.Interfaces
{
    public interface IGeminiService
    {
        Task<ServiceResult<string>> GenerateStoryReplyAsync(StoryChatRequestDto request, CancellationToken cancellationToken);
    }
}
