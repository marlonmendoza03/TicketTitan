using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketTitan.DTO;
using TicketTitan.Interfaces;

namespace TicketTitan.Controllers
{
    [ApiController]
    [Route("api/story/chat")]
    public class StoryChatController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public StoryChatController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] StoryChatRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _geminiService.GenerateStoryReplyAsync(request, cancellationToken);

            if (!result.IsSuccess || result.Value is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: result.ErrorMessage ?? "Gemini request failed.");
            }

            var reply = new ChatMessageDto(
                Id: Guid.NewGuid().ToString(),
                Role: "assistant",
                Content: result.Value,
                CreatedAt: DateTime.UtcNow);

            return Ok(new StoryChatResponseDto(reply));
        }
    }
}
