using Microsoft.EntityFrameworkCore;
using TicketTitan.Common;
using TicketTitan.Data;
using TicketTitan.DTO;
using TicketTitan.Interfaces;
using TicketTitan.Models;

namespace TicketTitan.Services
{
    public class StoryChatService : IStoryChatService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IGeminiChatService _geminiChatService;

        public StoryChatService(ApplicationDbContext dbContext, IGeminiChatService geminiChatService)
        {
            _dbContext = dbContext;
            _geminiChatService = geminiChatService;
        }

        public async Task<ServiceResult<StoryChatSessionDto>> CreateSessionAsync(string userId, string? title)
        {
            var session = new StoryChatSession
            {
                UserId = userId,
                Title = string.IsNullOrWhiteSpace(title) ? "New story chat" : title
            };

            _dbContext.StoryChatSessions.Add(session);
            await _dbContext.SaveChangesAsync();

            var reply = new StoryChatSessionDto
            {
                Id = session.Id,
                Title = session.Title,
                Messages = new List<StoryChatMessageDto>()
            };

            return ServiceResult<StoryChatSessionDto>.Success(reply);
        }

        public async Task<ServiceResult<StoryChatSessionDto?>> GetSessionAsync(Guid sessionId, string userId)
        {
            var session = await _dbContext.StoryChatSessions
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);

            if (session is null)
                return ServiceResult<StoryChatSessionDto>.Failure("Session Not Found");

            var reply = new StoryChatSessionDto
            {
                Id = session.Id,
                Title = session.Title,
                Messages = session.Messages
                    .OrderBy(x => x.CreatedAt)
                    .Select(x => new StoryChatMessageDto
                    {
                        Role = x.Role,
                        Content = x.Content
                    }).ToList()
            };

            return ServiceResult<StoryChatSessionDto>.Success(reply);
        }

        public async Task<ServiceResult<StoryChatSessionDto>> SendMessageAsync(Guid sessionId, string userId, string content, string model = "gemini-3.5-flash-lite")
        {
            var session = await _dbContext.StoryChatSessions
                    .Include(x => x.Messages)
                    .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);

            if (session is null)
                return ServiceResult<StoryChatSessionDto>.Failure("Session not found");

            var userMessage = new StoryChatMessage
            {
                SessionId = session.Id,
                Role = RoleType.User,
                Content = content
            };

            _dbContext.StoryChatMessages.Add(userMessage);

            var history = session.Messages
                .OrderBy(x => x.CreatedAt)
                .Select(x => (x.Role.ToGeminiRole(), x.Content))
                .ToList();

            history.Add((RoleType.User.ToGeminiRole(), content));

            var reply = await _geminiChatService.SendChatAsync(history, model, CancellationToken.None);

            if (!reply.IsSuccess)
                return ServiceResult<StoryChatSessionDto>.Failure(reply.ErrorMessage);

            var assistantMessage = new StoryChatMessage
            {
                SessionId = session.Id,
                Role = RoleType.Assistant,
                Content = reply.Value.content
            };

            _dbContext.StoryChatMessages.Add(assistantMessage);
            session.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var updatedSession = await _dbContext.StoryChatSessions
                .Include(x => x.Messages)
                .FirstAsync(x => x.Id == session.Id);

            var response = new StoryChatSessionDto
            {
                Id = updatedSession.Id,
                Title = updatedSession.Title,
                Messages = updatedSession.Messages
                    .OrderBy(x => x.CreatedAt)
                    .Select(x => new StoryChatMessageDto
                    {
                        Role = x.Role,
                        Content = x.Content
                    }).ToList()
            };

            return ServiceResult<StoryChatSessionDto>.Success(response);
        }
    }
}
