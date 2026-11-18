using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketTitan.DTO;
using TicketTitan.Interfaces;

namespace TicketTitan.Controllers
{
    [ApiController]
    [Route("api/story-chat")]
    [Authorize]
    public class StoryChatController : ControllerBase
    {
        private readonly IStoryChatService _storyChatService;
        public StoryChatController(IStoryChatService storyChatService)
        {
            _storyChatService = storyChatService;
        }

        [HttpPost("sessions")]
        public async Task<ActionResult<StoryChatSessionDto>> CreateSession([FromBody] StoryChatCreateRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var session = await _storyChatService.CreateSessionAsync(userId, request.Title);
            if (!session.IsSuccess)
            {
                return BadRequest(session.ErrorMessage);
            }
            return Ok(session.Value);
        }

        [HttpGet("sessions")]
        public async Task<ActionResult<List<StoryChatSessionDto>>> GetSessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var sessions = await _storyChatService.GetSessionsAsync(userId);
            return Ok(sessions);
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<ActionResult<StoryChatSessionDto>> GetSession(Guid sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var session = await _storyChatService.GetSessionAsync(sessionId, userId);

            if (!session.IsSuccess)
                return BadRequest(session.ErrorMessage);

            if (session.Value is null)
                return NotFound();

            return Ok(session.Value);
        }

        [HttpPost("sessions/{sessionId}/messages")]
        public async Task<ActionResult<StoryChatSessionDto>> SendMessage(Guid sessionId, [FromBody] StoryChatMessageDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Message content is required.");

            var session = await _storyChatService.SendMessageAsync(sessionId, userId, request.Content);
            if (!session.IsSuccess)
                return BadRequest(session.ErrorMessage);

            return Ok(session.Value);
        }

        [HttpPost("sessions/message")]
        public async Task<ActionResult<StoryChatSessionDto>> SendInitialMessage([FromBody] StoryChatMessageDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Message content is required.");

            var initialMessage = await _storyChatService.StartChatAsync(userId, request.Content, request.StorySetup, "gemini-3.5-flash-lite");

            if (!initialMessage.IsSuccess)
                return BadRequest(initialMessage.ErrorMessage);

            return Ok(initialMessage.Value);
        }   

    }
}
