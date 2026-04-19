using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Exceptions;
using AstralDiaryApi.Models.DTOs.Attachments;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Oci.ObjectstorageService;
using Oci.ObjectstorageService.Requests;

namespace AstralDiaryApi.Services.Implementations
{
    public class FileStorageService : IFileStorageService
    {
        private readonly AppDbContext _dbContext;
        private readonly ObjectStorageClient _objectStorageClient;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _namespace;
        private readonly string _bucketName;
        private readonly bool _testInLocalDir;

        public FileStorageService(
            AppDbContext dbContext,
            ObjectStorageClient objectStorageClient,
            IHostEnvironment env,
            IConfiguration configuration,
            ILogger<FileStorageService> logger
        )
        {
            _dbContext = dbContext;
            _objectStorageClient = objectStorageClient;
            _env = env;
            _configuration = configuration;
            _logger = logger;
            _bucketName = _configuration["OciStorage:BucketName"];
            _testInLocalDir = bool.Parse(_configuration["TestInLocalDir"]) || false;

            try
            {
                var namespaceResponse = _objectStorageClient.GetNamespace(
                    new GetNamespaceRequest()
                );
                _namespace = namespaceResponse.Result.Value;

                _logger.LogInformation(
                    $"OCI Storage Service initialized. Namespace: {_namespace}, Bucket: {_bucketName}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize OCI client: {ex.Message}");
                throw;
            }
        }

        public async Task<(string, string)> SaveAttachment(
            IFormFile attachmentFile,
            IFormFile thumbnailFile,
            string entityId,
            Guid userId
        )
        {
            var attachmentId = $"{entityId}-{Guid.NewGuid().ToString()}";
            var thumbnailId = attachmentId + "-thumbnail";

            if (_env.IsProduction() || !_testInLocalDir)
            {
                byte[] attachmentData;
                byte[] thumbnailData;

                using (var ms = new MemoryStream())
                {
                    await attachmentFile.CopyToAsync(ms);
                    attachmentData = ms.ToArray();
                }

                using (var ms = new MemoryStream())
                {
                    await thumbnailFile.CopyToAsync(ms);
                    thumbnailData = ms.ToArray();
                }

                try
                {
                    _logger.LogInformation(
                        $"Uploading file: {attachmentId} to bucket: {_bucketName}"
                    );

                    // Upload attachment
                    var putObjectRequestAttachment = new PutObjectRequest
                    {
                        NamespaceName = _namespace,
                        BucketName = _bucketName,
                        ObjectName = attachmentId,
                        PutObjectBody = new MemoryStream(attachmentData),
                        ContentType = "application/octet-stream",
                    };
                    await _objectStorageClient.PutObject(putObjectRequestAttachment);

                    // Upload thumbnail
                    var putObjectRequestThumbnail = new PutObjectRequest
                    {
                        NamespaceName = _namespace,
                        BucketName = _bucketName,
                        ObjectName = thumbnailId,
                        PutObjectBody = new MemoryStream(thumbnailData),
                        ContentType = "application/octet-stream",
                    };
                    await _objectStorageClient.PutObject(putObjectRequestThumbnail);

                    _logger.LogInformation($"File uploaded successfully: {attachmentId}");

                    return (attachmentId, thumbnailId);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error uploading file {attachmentId}: {ex.Message}");
                    throw;
                }
            }
            else
            {
                var basePath = Path.Combine("Storage", "attachments", userId.ToString(), entityId);

                var attachmentFilePath = Path.Combine(basePath, attachmentId);
                var thumbnailFilePath = Path.Combine(basePath, thumbnailId);

                try
                {
                    _logger.LogInformation($"Saving file: {attachmentId} to local storage");

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

                return (attachmentId, thumbnailId);
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

            var fileIdQuery = attachmentType.ToLower() switch
            {
                "thumbnail" => query.Select(e => e.ThumbnailId),
                "attachment" => query.Select(e => e.AttachmentId),
                _ => throw new ArgumentException("Invalid attachment type."),
            };

            var fileId = await fileIdQuery.FirstOrDefaultAsync();

            if (fileId == null)
                throw new NotFoundException("File not found");

            if (_env.IsProduction() || !_testInLocalDir)
                return await GetFileOciAsync(fileId);
            else
                return await GetFileLocal(userId, entityId, fileId);
        }

        private async Task<FileDownloadResult> GetFileOciAsync(string fileId)
        {
            try
            {
                _logger.LogInformation($"Fetching file: {fileId} from bucket: {_bucketName}");

                var getObjectRequest = new GetObjectRequest
                {
                    NamespaceName = _namespace,
                    BucketName = _bucketName,
                    ObjectName = fileId,
                };

                var response = await _objectStorageClient.GetObject(getObjectRequest);

                var memoryStream = new MemoryStream();
                await response.InputStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                _logger.LogInformation($"File fetched successfully: {fileId}");

                return new FileDownloadResult { FileStream = memoryStream, FileName = fileId };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching file {fileId}: {ex.Message}");
                throw;
            }
        }

        private async Task<FileDownloadResult> GetFileLocal(
            Guid userId,
            string entityId,
            string fileId
        )
        {
            var basePath = Path.Combine("Storage", "attachments", userId.ToString(), entityId);
            var filePath = Path.Combine(basePath, fileId);

            try
            {
                if (!File.Exists(filePath))
                {
                    throw new NotFoundException("File not found");
                }

                var fileStream = new FileStream(filePath, FileMode.Open);
                var fileNameSafe = Path.GetFileName(filePath);

                return new FileDownloadResult { FileStream = fileStream, FileName = fileNameSafe };
            }
            catch (Exception ex)
            {
                throw new IOException($"Error downloading file: {ex.Message}");
            }
        }

        public async Task DeleteSavedAttachment(
            Guid userId,
            string entityId,
            string attachmentId,
            string thumbnailId
        )
        {
            if (String.IsNullOrEmpty(entityId) || String.IsNullOrEmpty(attachmentId))
                return;

            if (_env.IsProduction() || !_testInLocalDir)
            {
                await DeleteFileOciAsync(attachmentId);
                await DeleteFileOciAsync(thumbnailId);

                return;
            }
            else
            {
                var basePath = Path.Combine("Storage", "attachments", userId.ToString(), entityId);

                try
                {
                    if (!Directory.Exists(basePath))
                        return;

                    Directory.Delete(basePath, recursive: true);
                    return;
                }
                catch (Exception ex)
                {
                    throw new IOException($"Error deleting attachment: {ex.Message}");
                }
            }
        }

        private async Task DeleteFileOciAsync(string fileId)
        {
            try
            {
                _logger.LogInformation($"Deleting file: {fileId} from bucket: {_bucketName}");

                var deleteObjectRequest = new DeleteObjectRequest
                {
                    NamespaceName = _namespace,
                    BucketName = _bucketName,
                    ObjectName = fileId,
                };

                await _objectStorageClient.DeleteObject(deleteObjectRequest);
                _logger.LogInformation($"File deleted successfully: {fileId}");

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting file {fileId}: {ex.Message}");
                throw;
            }
        }
    }
}
