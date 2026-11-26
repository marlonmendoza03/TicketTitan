using Microsoft.EntityFrameworkCore;
using TicketTitan.Common;
using TicketTitan.Data;
using TicketTitan.Interfaces;
using TicketTitan.Models;

namespace TicketTitan.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;
        private const string BucketName = "story-attachments";

        public AttachmentService(ApplicationDbContext dbContext, HttpClient httpClient, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _httpClient = httpClient;
            _configuration = configuration;

            _supabaseUrl = _configuration["Supabase__Url"] ?? Environment.GetEnvironmentVariable("Supabase__Url") ?? string.Empty;
            _supabaseKey = _configuration["Supabase__AnonKey"] ?? Environment.GetEnvironmentVariable("Supabase__AnonKey") ?? string.Empty;
        }

        public async Task<ServiceResult<StoryChatAttachment>> UploadAttachmentAsync(Guid sessionId, string userId, string fileName, Stream fileStream, long fileSize, AttachmentType type, string contentType)
        {
            var session = await _dbContext.StoryChatSessions
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId);

            if (session is null)
                return ServiceResult<StoryChatAttachment>.Failure("Session not found");

            var uniqueName = $"{sessionId}/{Guid.NewGuid()}_{fileName}";
            var storagePath = uniqueName;

            // Upload to Supabase Storage REST API
            if (string.IsNullOrWhiteSpace(_supabaseUrl) || string.IsNullOrWhiteSpace(_supabaseKey))
            {
                return ServiceResult<StoryChatAttachment>.Failure("Unable to connect to Supabase.");
            }

            var uploadUrl = $"{_supabaseUrl.TrimEnd('/')}/storage/v1/object/{BucketName}/{storagePath}";

            using var content = new StreamContent(fileStream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

            using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
            request.Headers.Add("apikey", _supabaseKey);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return ServiceResult<StoryChatAttachment>.Failure($"Supabase storage upload failed: {response.StatusCode} - {err}");
            }

            var downloadUrl = GetDownloadUrl(storagePath);

            var attachment = new StoryChatAttachment
            {
                SessionId = sessionId,
                UserId = userId,
                Type = type,
                FileName = fileName,
                StoragePath = storagePath,
                DownloadUrl = downloadUrl,
                FileSize = fileSize,
                ContentType = contentType
            };

            _dbContext.StoryChatAttachments.Add(attachment);
            await _dbContext.SaveChangesAsync();

            // Return the full attachment entity (including the generated Id)
            return ServiceResult<StoryChatAttachment>.Success(attachment);
        }

        public async Task<ServiceResult<bool>> DeleteAttachmentAsync(Guid sessionId, string userId, Guid attachmentId)
        {
            var attachment = await _dbContext.StoryChatAttachments
                .FirstOrDefaultAsync(x => x.Id == attachmentId && x.SessionId == sessionId && x.UserId == userId);

            if (attachment is null)
                return ServiceResult<bool>.Failure("Attachment not found");

            // Delete from Supabase Storage
            if (string.IsNullOrWhiteSpace(_supabaseUrl) || string.IsNullOrWhiteSpace(_supabaseKey) || string.IsNullOrWhiteSpace(attachment.StoragePath))
            {
                return ServiceResult<bool>.Failure("Unable to connect to Supabase.");
            }

            var deleteUrl = $"{_supabaseUrl.TrimEnd('/')}/storage/v1/object/{BucketName}";
            using var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
            request.Headers.Add("apikey", _supabaseKey);
            request.Content = JsonContent.Create(new { prefixes = new[] { attachment.StoragePath } });

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return ServiceResult<bool>.Failure($"Supabase storage delete failed: {response.StatusCode} - {err}");
            }

            _dbContext.StoryChatAttachments.Remove(attachment);
            await _dbContext.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public string GetDownloadUrl(string storagePath)
        {
            if (string.IsNullOrWhiteSpace(_supabaseUrl))
                return storagePath;

            return $"{_supabaseUrl.TrimEnd('/')}/storage/v1/object/public/{BucketName}/{storagePath}";
        }
    }
}
