using System.Drawing;
using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Exceptions;
using AstralDiaryApi.Models.DTOs.Attachments;
using AstralDiaryApi.Models.DTOs.Entries.Delete;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Services.Interfaces;
using ImageMagick;
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

        public async Task<AttachmentObjResponse> SaveAttachment(IFormFile file, string entityId)
        {
            if (_env.IsDevelopment())
            {
                var newFileName = Guid.NewGuid().ToString();
                var fileExtension = Path.GetExtension(file.FileName);
                var completeNewFileName = newFileName + fileExtension;

                var filePath = Path.Combine(
                    "Storage",
                    "attachments",
                    entityId,
                    completeNewFileName
                );
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                stream.Position = 0;
                string thumbnailPath = await CreateThumbnail(
                    stream,
                    entityId,
                    newFileName,
                    fileExtension
                );

                var response = new AttachmentObjResponse
                {
                    FilePath = filePath,
                    ThumbnailPath = thumbnailPath,
                    InternalFileName = completeNewFileName,
                    OriginalFileName = file.FileName,
                };

                return response;
            }
            else
            {
                // Upload to Oracle Object Storage
                throw new NotImplementedException();
            }
        }

        private async Task<string> CreateThumbnail(
            FileStream stream,
            string entityId,
            string fileName,
            string fileExtension
        )
        {
            var thumbnailFileName = $"{fileName}-thumbnail";
            var completeThumbnailFileName = thumbnailFileName + fileExtension;

            using var thumbnail = new MagickImage(stream);
            var size = new MagickGeometry(100, 100);
            thumbnail.Resize(size);
            var thumbnailPath = Path.Combine(
                "Storage",
                "attachments",
                entityId,
                "thumbnails",
                completeThumbnailFileName
            );
            Directory.CreateDirectory(Path.GetDirectoryName(thumbnailPath));
            thumbnail.Write($"{thumbnailPath}");

            return thumbnailPath;
        }

        public async Task<FileDownloadResult> GetAttachment(
            Guid userId,
            string entityId,
            string internalFileName
        )
        {
            var attachmentPath = "";
            if (entityId.StartsWith("entry-"))
            {
                attachmentPath = await GetAttachmentPath<Entry>(userId, entityId, internalFileName);
            }
            else if (entityId.StartsWith("draft-"))
            {
                attachmentPath = await GetAttachmentPath<Draft>(userId, entityId, internalFileName);
            }
            else
            {
                throw new NotFoundException("File not found");
            }

            if (attachmentPath == null)
                throw new NotFoundException("File not found");

            //var filePath = Path.Combine("Storage", "attachments", entityId, internalFileName);

            return await GetFile(attachmentPath, internalFileName);
        }

        public async Task<FileDownloadResult> GetThumbnail(
            Guid userId,
            string entityId,
            string internalFileName
        )
        {
            var thumbnailPath = "";
            if (entityId.StartsWith("entry-"))
            {
                thumbnailPath = await GetThumbnailPath<Entry>(userId, entityId, internalFileName);
            }
            else if (entityId.StartsWith("draft-"))
            {
                thumbnailPath = await GetThumbnailPath<Draft>(userId, entityId, internalFileName);
            }
            else
            {
                throw new NotFoundException("File not found");
            }

            if (thumbnailPath == null)
                throw new NotFoundException("File not found");

            //var fileName = Path.GetFileNameWithoutExtension(internalFileName);
            //var fileExtension = Path.GetExtension(internalFileName);

            //var filePath = Path.Combine(
            //    "Storage",
            //    "attachments",
            //    entityId,
            //    "thumbnails",
            //    $"{fileName}-thumbnail{fileExtension}"
            //);

            return await GetFile(thumbnailPath, internalFileName);
        }

        private async Task<string?> GetThumbnailPath<TEntity>(
            Guid userId,
            string entityId,
            string internalFileName
        )
            where TEntity : class, IAttachmentSource, IEntityIdSource
        {
            return await _dbContext
                .Set<TEntity>()
                .AsNoTracking()
                .Where(e => e.EntityId == entityId && e.UserId == userId)
                .SelectMany(e => e.Attachments)
                .Where(a => a.InternalFileName == internalFileName)
                .Select(a => a.ThumbnailPath)
                .FirstOrDefaultAsync();
        }

        private async Task<string?> GetAttachmentPath<TEntity>(
            Guid userId,
            string entityId,
            string internalFileName
        )
            where TEntity : class, IAttachmentSource, IEntityIdSource
        {
            return await _dbContext
                .Set<TEntity>()
                .AsNoTracking()
                .Where(e => e.EntityId == entityId && e.UserId == userId)
                .SelectMany(e => e.Attachments)
                .Where(a => a.InternalFileName == internalFileName)
                .Select(a => a.FilePath)
                .FirstOrDefaultAsync();
        }

        private async Task<FileDownloadResult> GetFile(string filePath, string internalFileName)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new NotFoundException("File not found");
                }

                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var fileNameSafe = Path.GetFileName(internalFileName);
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

        public Task<DeleteAttachmentsResult> DeleteAttachmentAndThumbnail(
            List<AttachmentComparisonDto> attachmentsToDelete,
            string entityId
        )
        {
            var basePath = Path.Combine("Storage", "attachments", entityId);
            var result = new DeleteAttachmentsResult { SuccessAll = true };

            foreach (var attachment in attachmentsToDelete)
            {
                try
                {
                    var safeFileName = Path.GetFileName(attachment.InternalFileName);
                    var filePath = Path.Combine(basePath, safeFileName);

                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(safeFileName);
                    var fileExtension = Path.GetExtension(safeFileName);
                    var thumbnailPath = Path.Combine(
                        basePath,
                        "thumbnails",
                        $"{fileNameWithoutExt}-thumbnail{fileExtension}"
                    );

                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    if (File.Exists(thumbnailPath))
                        File.Delete(thumbnailPath);

                    result.DeletedCount++;
                }
                catch (Exception ex)
                {
                    result.FailedFiles.Add(attachment);
                }
            }

            if (result.FailedFiles.Count > 0 || result.DeletedCount < attachmentsToDelete.Count)
                result.SuccessAll = false;

            return Task.FromResult(result);
        }

        public Task<DeleteAllAttachmentsResult> DeleteAllAttachment(string entityId)
        {
            var basePath = Path.Combine("Storage", "attachments", entityId);
            var result = new DeleteAllAttachmentsResult();

            try
            {
                if (!Directory.Exists(basePath))
                {
                    result.Success = false;
                    return Task.FromResult(result);
                }

                Directory.Delete(basePath, recursive: true);
                result.Success = true;

                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;

                return Task.FromResult(result);
            }
        }
    }
}
