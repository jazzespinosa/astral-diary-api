using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Models.DTOs.Attachments;
using AstralDiaryApi.Models.DTOs.Entries.Delete;

namespace AstralDiaryApi.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<AttachmentObjResponse> SaveAttachment(IFormFile file, string sourceId);
        Task<FileDownloadResult> GetThumbnail(
            Guid userId,
            string entityId,
            string internalFileName
        );
        Task<FileDownloadResult> GetAttachment(
            Guid userId,
            string entityId,
            string internalFileName
        );
        Task<DeleteAttachmentsResult> DeleteAttachmentAndThumbnail(
            List<AttachmentComparisonDto> attachmentsToDelete,
            string entityId
        );
        Task<DeleteAllAttachmentsResult> DeleteAllAttachment(string entityId);
    }
}
