using Microsoft.AspNetCore.Mvc;
using TicketTitan.DTO;
using TicketTitan.Interfaces;

namespace TicketTitan.Controllers
{
    public class GeminiChatController : ControllerBase
    {
        private readonly IGeminiChatService _geminiChatService;
        public GeminiChatController(IGeminiChatService geminiChatService)
        {
            _geminiChatService = geminiChatService;
        }

        [HttpPost("api/chat")]
        public async Task<ActionResult<GeminiChatResponseDto>> Chat([FromBody] GeminiChatRequestDto request, CancellationToken cancellationToken)
        {
            if (request.Messages == null || request.Messages.Count == 0)
            {
                return BadRequest("At least one message is required.");
            }

            var prompt = request.Messages.Last().Content;

            var reply = await _geminiChatService.SendChatAsync(request.Messages.Select(m => (m.Role, m.Content)).ToList(), request.Model, cancellationToken);

            if (!reply.IsSuccess)
            {
                return BadRequest(reply.ErrorMessage);
            }

            return Ok(reply.Value);
        }
    }
}
