using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Models.DTOs.Attachments;

namespace AstralDiaryApi.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<AttachmentObjResponse> SaveAttachment(IFormFile file, string sourceId);
        Task<FileDownloadResult> GetEntryThumbnail(
            Guid userId,
            string entryId,
            string internalFileName
        );
    }
}
