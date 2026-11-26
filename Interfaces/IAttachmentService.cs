using TicketTitan.Common;
using TicketTitan.Models;

namespace TicketTitan.Interfaces
{
    public interface IAttachmentService
    {
        Task<ServiceResult<StoryChatAttachment>> UploadAttachmentAsync(Guid sessionId, string userId, string fileName, Stream fileStream, long fileSize, AttachmentType type, string contentType);
        Task<ServiceResult<bool>> DeleteAttachmentAsync(Guid sessionId, string userId, Guid attachmentId);
        string GetDownloadUrl(string storagePath);
    }
}
