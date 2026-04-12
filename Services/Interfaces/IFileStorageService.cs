using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Models.DTOs.Attachments;

namespace AstralDiaryApi.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<AttachmentObject> SaveAttachment(
            IFormFile attachmentFile,
            IFormFile thumbnailFile,
            string entityId,
            Guid userId
        );
        Task<FileDownloadResult> GetAttachmentFile<TEntity>(
            Guid userId,
            string entityId,
            string attachmentType,
            string attachmentId
        )
            where TEntity : class, IEntityIdSource;
        Task DeleteSavedAttachment(string entityId, Guid userId);
    }
}
