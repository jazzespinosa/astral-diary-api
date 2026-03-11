using System.Drawing;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Exceptions;
using AstralDiaryApi.Models.DTOs.Attachments;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Services.Interfaces;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

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

        public async Task<AttachmentObjResponse> SaveAttachment(IFormFile file, string sourceId)
        {
            //if (_env.IsDevelopment())
            //{
            var newFileName = Guid.NewGuid().ToString();
            var fileExtension = Path.GetExtension(file.FileName);
            var completeNewFileName = newFileName + fileExtension;

            var filePath = Path.Combine("Storage/attachments", sourceId, completeNewFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            stream.Position = 0;
            string thumbnailPath = await CreateThumbnail(
                stream,
                sourceId,
                newFileName,
                fileExtension
            );

            return new AttachmentObjResponse
            {
                FilePath = filePath,
                ThumbnailPath = thumbnailPath,
                InternalFileName = completeNewFileName,
            };

            //}
            //else
            //{
            //    // Upload to Oracle Object Storage
            //}
        }

        private async Task<string> CreateThumbnail(
            FileStream stream,
            string sourceId,
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
                "Storage/attachments",
                sourceId,
                "thumbnails",
                completeThumbnailFileName
            );
            Directory.CreateDirectory(Path.GetDirectoryName(thumbnailPath));
            thumbnail.Write($"{thumbnailPath}");

            return thumbnailPath;
        }

        public async Task GetAttachment(Guid userId, string internalFileName)
        {
            //try
            //{
            //    var filePath = Path.Combine("wwwroot/attachments", fileName); // Adjust path as needed

            //    if (!System.IO.File.Exists(filePath))
            //    {
            //        return NotFound("File not found");
            //    }

            //    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            //    var fileNameSafe = Path.GetFileName(fileName);

            //    return File(
            //        fileBytes,
            //        "application/octet-stream", // or specific MIME type
            //        fileNameSafe
            //    );
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest($"Error downloading file: {ex.Message}");
            //}
        }

        public async Task<FileDownloadResult> GetEntryThumbnail(
            Guid userId,
            string entryId,
            string internalFileName
        )
        {
            var thumbnailPath = await _dbContext
                .Entries.Where(e => e.EntityId == entryId && e.UserId == userId)
                .Include(e => e.Attachments)
                .Select(e =>
                    e.Attachments.Where(a => a.InternalFileName == internalFileName)
                        .Select(a => a.ThumbnailPath)
                        .FirstOrDefault()
                )
                .FirstOrDefaultAsync();

            if (thumbnailPath == null)
                throw new NotFoundException("File not found");

            return await GetThumbnailFile(entryId, internalFileName);
        }

        private async Task<FileDownloadResult> GetFile(string sourceId, string internalFileName)
        {
            try
            {
                var filePath = Path.Combine("Storage/attachments", sourceId, internalFileName);

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
                throw new ArgumentException($"Error downloading file: {ex.Message}");
            }
        }

        private async Task<FileDownloadResult> GetThumbnailFile(
            string sourceId,
            string internalFileName
        )
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(internalFileName);
                var fileExtension = Path.GetExtension(internalFileName);

                var filePath = Path.Combine(
                    "Storage/attachments",
                    sourceId,
                    "thumbnails",
                    $"{fileName}-thumbnail{fileExtension}"
                );

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
                throw new ArgumentException($"Error downloading file: {ex.Message}");
            }
        }

        public async Task RenameAttachment(string path, string sourceId) { }

        public async Task RenameThumbnail(string path, string sourceId) { }

        public async Task RemoveAttachment(string path, string sourceId) { }

        public async Task RemoveThumbnail(string path, string sourceId) { }
    }
}
