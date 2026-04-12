using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Exceptions;
using AstralDiaryApi.Models.DTOs.Attachments;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Services.Implementations
{
    public class FileStorageService : IFileStorageService
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public FileStorageService(AppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        public async Task<AttachmentObject> SaveAttachment(
            IFormFile attachmentFile,
            IFormFile thumbnailFile,
            string entityId,
            Guid userId
        )
        {
            if (_env.IsDevelopment())
            {
                var attachmentId = Guid.NewGuid().ToString();
                var fileExtension = Path.GetExtension(attachmentFile.FileName);
                var completeAttachmentFileName = attachmentId + fileExtension;

                var thumbnailExtension = Path.GetExtension(thumbnailFile.FileName);
                var completeThumbnailFileName = attachmentId + "-thumbnail" + thumbnailExtension;

                var basePath = Path.Combine(
                    "Storage",
                    "attachments",
                    userId.ToString().ToLower(),
                    entityId
                );

                var attachmentFilePath = Path.Combine(basePath, completeAttachmentFileName);
                var thumbnailFilePath = Path.Combine(basePath, completeThumbnailFileName);

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(attachmentFilePath));
                    using var streamAttachment = new FileStream(
                        attachmentFilePath,
                        FileMode.Create
                    );
                    await attachmentFile.CopyToAsync(streamAttachment);

                    using var streamThumbnail = new FileStream(thumbnailFilePath, FileMode.Create);
                    await thumbnailFile.CopyToAsync(streamThumbnail);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Error saving attachment: {ex.Message}");
                }

                var response = new AttachmentObject
                {
                    AttachmentPath = attachmentFilePath,
                    ThumbnailPath = thumbnailFilePath,
                    AttachmentId = attachmentId,
                };

                return response;
            }
            else
            {
                // Upload to Oracle Object Storage
                throw new NotImplementedException();
            }
        }

        public async Task<FileDownloadResult> GetAttachmentFile<TEntity>(
            Guid userId,
            string entityId,
            string attachmentType,
            string attachmentId
        )
            where TEntity : class, IEntityIdSource
        {
            var query = _dbContext
                .Set<TEntity>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(e =>
                    e.UserId == userId && e.EntityId == entityId && e.AttachmentId == attachmentId
                );

            var pathQuery = attachmentType.ToLower() switch
            {
                "thumbnail" => query.Select(e => e.ThumbnailPath),
                "attachment" => query.Select(e => e.AttachmentPath),
                _ => throw new ArgumentException("Invalid attachment type."),
            };

            var path = await pathQuery.FirstOrDefaultAsync();

            if (path == null)
                throw new NotFoundException("File not found");

            return await GetFile(path);
        }

        private async Task<FileDownloadResult> GetFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new NotFoundException("File not found");
                }

                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var fileNameSafe = Path.GetFileName(filePath);
                var response = new FileDownloadResult
                {
                    FileBytes = fileBytes,
                    FileName = fileNameSafe,
                };
                return response;
            }
            catch (Exception ex)
            {
                throw new IOException($"Error downloading file: {ex.Message}");
            }
        }

        public Task DeleteSavedAttachment(string entityId, Guid userId)
        {
            var basePath = Path.Combine(
                "Storage",
                "attachments",
                userId.ToString().ToLower(),
                entityId
            );

            try
            {
                if (!Directory.Exists(basePath))
                    return Task.CompletedTask;

                Directory.Delete(basePath, recursive: true);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new IOException($"Error deleting attachment: {ex.Message}");
            }
        }
    }
}
