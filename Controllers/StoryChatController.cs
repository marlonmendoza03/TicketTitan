using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TicketTitan.DTO;
using TicketTitan.Interfaces;
using TicketTitan.Models;

namespace TicketTitan.Controllers
{
    [ApiController]
    [Route("api/story-chat")]
    [Authorize]
    public class StoryChatController : ControllerBase
    {
        private readonly IStoryChatService _storyChatService;
        private readonly IAttachmentService _attachmentService;
        public StoryChatController(IStoryChatService storyChatService, IAttachmentService attachmentService)
        {
            _storyChatService = storyChatService;
            _attachmentService = attachmentService;
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

        [HttpPost("sessions/{sessionId}/approve")]
        public async Task<ActionResult<StoryChatSessionDto>> ApproveSession(Guid sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _storyChatService.ApproveSessionAsync(sessionId, userId);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            if (result.Value is null)
                return NotFound();

            return Ok(result.Value);
        }

        [HttpPost("sessions/{sessionId}/attachments")]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<ActionResult<StoryAttachmentDto>> UploadAttachment(Guid sessionId, [FromForm] IFormFile file, [FromForm] AttachmentType type)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            if (file is null || file.Length == 0)
                return BadRequest("File is required.");

            var result = await _attachmentService.UploadAttachmentAsync(sessionId, userId, file.FileName, file.OpenReadStream(), file.Length, type, file.ContentType);
            if (!result.IsSuccess || result.Value is null)
                return BadRequest(result.ErrorMessage);

            // Return the attachment directly from the service - no need to re-query (fixes concurrency issue #5)
            var attachment = result.Value;
            var attachmentDto = new StoryAttachmentDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                Type = attachment.Type,
                DownloadUrl = attachment.DownloadUrl,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                UploadedAt = attachment.UploadedAt
            };

            return Ok(attachmentDto);
        }

        [HttpGet("sessions/{sessionId}/attachments")]
        public async Task<ActionResult<List<StoryAttachmentDto>>> GetAttachments(Guid sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var session = await _storyChatService.GetSessionAsync(sessionId, userId);
            if (!session.IsSuccess || session.Value is null)
                return BadRequest(session.ErrorMessage);

            return Ok(session.Value.Attachments);
        }

        [HttpDelete("sessions/{sessionId}/attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(Guid sessionId, Guid attachmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _attachmentService.DeleteAttachmentAsync(sessionId, userId, attachmentId);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return NoContent();
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> DeleteSession(Guid sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var result = await _storyChatService.DeleteSessionAsync(sessionId, userId);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return NoContent();
        }
    }
}
