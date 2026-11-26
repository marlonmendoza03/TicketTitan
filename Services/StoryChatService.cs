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
        private readonly IAttachmentService _attachmentService;

        public StoryChatService(ApplicationDbContext dbContext, IGeminiChatService geminiChatService, IAttachmentService attachmentService)
        {
            _dbContext = dbContext;
            _geminiChatService = geminiChatService;
            _attachmentService = attachmentService;
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
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                Status = session.Status,
                Messages = new List<StoryChatMessageDto>(),
                Attachments = new List<StoryAttachmentDto>()
            };

            return ServiceResult<StoryChatSessionDto>.Success(reply);
        }

        public async Task<List<StoryChatSessionDto>> GetSessionsAsync(string userId)
        {
            return await _dbContext.StoryChatSessions
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => new StoryChatSessionDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Status = x.Status,
                    MessageCount = x.Messages.Count
                })
                .ToListAsync();
        }

        public async Task<ServiceResult<StoryChatSessionDto?>> GetSessionAsync(Guid sessionId, string userId)
        {
            var session = await _dbContext.StoryChatSessions
                .Include(x => x.Messages)
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);

            if (session is null)
                return ServiceResult<StoryChatSessionDto>.Failure("Session Not Found");

            var reply = new StoryChatSessionDto
            {
                Id = session.Id,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                Status = session.Status,
                MessageCount = session.Messages.Count,
                Attachments = session.Attachments
                    .OrderBy(x => x.UploadedAt)
                    .Select(x => new StoryAttachmentDto
                    {
                        Id = x.Id,
                        FileName = x.FileName,
                        Type = x.Type,
                        DownloadUrl = x.DownloadUrl,
                        FileSize = x.FileSize,
                        ContentType = x.ContentType,
                        UploadedAt = x.UploadedAt
                    }).ToList(),
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

            var response = new StoryChatSessionDto
            {
                Id = session.Id,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                Status = session.Status,
                MessageCount = session.Messages.Count + 2,
                Messages = session.Messages.OrderBy(x => x.CreatedAt).Select(x => new StoryChatMessageDto
                {
                    Role = x.Role,
                    Content = x.Content
                }).ToList()
            };

            return ServiceResult<StoryChatSessionDto>.Success(response);
        }

        public async Task<ServiceResult<StoryChatSessionDto>> StartChatAsync(string userId, string initialPrompt, StorySetup storySetup, string model = "gemini-3.5-flash-lite")
        {
            var title = storySetup.Category + " | Number of Episodes: " + storySetup.EpisodeCount + " | Duration per Episode: " + storySetup.EpisodeDurationInSec + " secs ";

            var sessionResult = await CreateSessionAsync(userId, title);
            if (!sessionResult.IsSuccess || sessionResult.Value is null)
                return ServiceResult<StoryChatSessionDto>.Failure(sessionResult.ErrorMessage);

            var sessionId = sessionResult.Value.Id;

            var contextPrompt =
                $"- Category: {storySetup.Category}\n" +
                $"- Number of Episodes: {storySetup.EpisodeCount}\n" +
                $"- Episode Duration: {storySetup.EpisodeDurationInSec} seconds\n" +
                "Each Episode should have clips not exceeding 10 seconds each as Veo will be used to generate those clips\n" +
                "Those 8 to 10 second clips will be stitched together to create an episode so if there is a narration on each clip, is should also be limited\n"+
                "Use this as the creative brief for the story.";

            var session = await _dbContext.StoryChatSessions
                .FirstAsync(x => x.Id == sessionId);

            var history = new List<(string Role, string Content)>
            {
                ("user", contextPrompt),
                ("user", initialPrompt)
            };

            var geminiResult = await _geminiChatService.SendChatAsync(history, model, CancellationToken.None);

            if (!geminiResult.IsSuccess)
                return ServiceResult<StoryChatSessionDto>.Failure(geminiResult.ErrorMessage);

            var userMessage = new StoryChatMessage
            {
                SessionId = sessionId,
                Session = session,
                Role = RoleType.User,
                Content = $"{contextPrompt}\n\n{initialPrompt}"
            };

            var assistantMessage = new StoryChatMessage
            {
                SessionId = session.Id,
                Session = session,
                Role = RoleType.Assistant,
                Content = geminiResult.Value.content
            };

            _dbContext.StoryChatMessages.AddRange(userMessage, assistantMessage);
            session.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var response = new StoryChatSessionDto
            {
                Id = session.Id,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                Status = session.Status,
                MessageCount = session.Messages.Count,
                Messages = session.Messages.OrderBy(x => x.CreatedAt).Select(x => new StoryChatMessageDto
                {
                    Role = x.Role,
                    Content = x.Content
                }).ToList()
            };

            return ServiceResult<StoryChatSessionDto>.Success(response);
        }

        public async Task<ServiceResult<StoryChatSessionDto>> ApproveSessionAsync(Guid sessionId, string userId)
        {
            var session = await _dbContext.StoryChatSessions
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);

            if (session is null)
                return ServiceResult<StoryChatSessionDto>.Failure("Session not found");

            session.Status = SessionStatus.Approved;
            session.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var response = new StoryChatSessionDto
            {
                Id = session.Id,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                Status = session.Status,
                MessageCount = session.Messages.Count,
                Attachments = session.Attachments
                    .OrderBy(x => x.UploadedAt)
                    .Select(x => new StoryAttachmentDto
                    {
                        Id = x.Id,
                        FileName = x.FileName,
                        Type = x.Type,
                        DownloadUrl = x.DownloadUrl,
                        FileSize = x.FileSize,
                        ContentType = x.ContentType,
                        UploadedAt = x.UploadedAt
                    }).ToList(),
                Messages = session.Messages.OrderBy(x => x.CreatedAt).Select(x => new StoryChatMessageDto
                {
                    Role = x.Role,
                    Content = x.Content
                }).ToList()
            };

            return ServiceResult<StoryChatSessionDto>.Success(response);
        }

        public async Task<ServiceResult<bool>> DeleteSessionAsync(Guid sessionId, string userId)
        {
            var session = await _dbContext.StoryChatSessions
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);

            if (session is null)
                return ServiceResult<bool>.Failure("Session not found");

            // Delete all attachments from Supabase Storage first
            foreach (var attachment in session.Attachments)
            {
                var deleteResult = await _attachmentService.DeleteAttachmentAsync(sessionId, userId, attachment.Id);
                if (!deleteResult.IsSuccess)
                {
                    // Log the failure but continue with other attachments
                    // In production, you might want to use a background job to retry
                    System.Diagnostics.Debug.WriteLine($"Failed to delete attachment {attachment.Id} from Supabase: {deleteResult.ErrorMessage}");
                }
            }

            // Delete the session (cascades to messages and attachments in DB)
            _dbContext.StoryChatSessions.Remove(session);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }
    }
}