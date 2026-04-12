using System.Linq.Expressions;
using System.Text.Json;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Exceptions;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Common.Generics
{
    public abstract class BaseEntryService<TEntity>
        where TEntity : class, IEntityIdSource
    {
        protected readonly AppDbContext _dbContext;
        protected readonly IFileStorageService _fileStorageService;

        public BaseEntryService(AppDbContext dbContext, IFileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
        }

        public abstract Task<IResponseDto> Create(Guid userId, IRequestDto newRequest);

        public abstract Task<IGetResponse> Get(Guid userId, string entityId);

        public abstract Task<IUpdateResponse> Update(Guid userId, IUpdateRequest updateRequest);

        protected async Task<IGetResponse?> FindByIdAsync(
            Guid userId,
            string entityId,
            DocuType docuType
        )
        {
            switch (docuType)
            {
                case DocuType.Entry:
                    return await _dbContext
                        .Entries.Where(e => e.EntityId == entityId && e.UserId == userId)
                        .Select(GetEntryProjection())
                        .FirstOrDefaultAsync();
                case DocuType.Draft:
                    return await _dbContext
                        .Drafts.Where(e => e.EntityId == entityId && e.UserId == userId)
                        .Select(GetDraftProjection())
                        .FirstOrDefaultAsync();
                default:
                    throw new ArgumentException("Invalid entry type.");
            }
        }

        protected async Task<TEntity?> FindEntityByIdAsync(Guid userId, string entityId)
        {
            return await _dbContext
                .Set<TEntity>()
                .FirstOrDefaultAsync(e => e.EntityId == entityId && e.UserId == userId);
        }

        protected Task UpdateContentsAsync(
            Guid userId,
            TEntity entity,
            IRequestDto updateEntryRequest
        )
        {
            if (entity == null)
                throw new NotFoundException("Entry does not exist.");

            entity.Date = updateEntryRequest.Date;
            entity.Mood = updateEntryRequest.Mood;
            entity.EncryptedContent = updateEntryRequest.EncryptedContent;
            entity.ContentIv = updateEntryRequest.ContentIv;
            entity.ContentSalt = updateEntryRequest.ContentSalt;
            entity.ModifiedAt = DateTime.UtcNow;

            return Task.CompletedTask;
        }

        protected async Task UpdateAttachmentsAsync(
            Guid userId,
            string entityId,
            TEntity entity,
            IRequestDto updateEntryRequest
        )
        {
            var currentAttachmentId = entity.AttachmentId;

            if (currentAttachmentId == null && updateEntryRequest.EncryptedAttachments == null)
                return;

            if (updateEntryRequest.EncryptedAttachments == null)
            {
                entity.AttachmentId = null;
                entity.AttachmentPath = null;
                entity.ThumbnailPath = null;
                entity.AttachmentHash = null;

                await _fileStorageService.DeleteSavedAttachment(entityId, userId);

                return;
            }

            if (currentAttachmentId == null)
            {
                await AddAttachmentsAsync(entity, updateEntryRequest, userId);
                return;
            }

            if (!AreHashesIdentical(entity.AttachmentHash, updateEntryRequest.AttachmentHash))
            {
                await _fileStorageService.DeleteSavedAttachment(entityId, userId);
                await AddAttachmentsAsync(entity, updateEntryRequest, userId);

                return;
            }

            return;
        }

        protected async Task AddAttachmentsAsync(
            TEntity entity,
            IRequestDto newRequest,
            Guid userId
        )
        {
            if (newRequest.EncryptedAttachments != null)
            {
                var path = await _fileStorageService.SaveAttachment(
                    newRequest.EncryptedAttachments,
                    newRequest.EncryptedThumbnails,
                    entity.EntityId,
                    userId
                );
                entity.AttachmentId = path.AttachmentId;
                entity.AttachmentPath = path.AttachmentPath;
                entity.ThumbnailPath = path.ThumbnailPath;
                entity.AttachmentHash = newRequest.AttachmentHash;

                await _dbContext.SaveChangesAsync();
            }
        }

        private bool AreHashesIdentical(string currentHashJson, string newHashJson)
        {
            if (currentHashJson == null || newHashJson == null)
                return false;

            var newHashes = JsonSerializer.Deserialize<List<string>>(newHashJson);
            var currentHashes = JsonSerializer.Deserialize<List<string>>(currentHashJson);

            if (newHashes == null || currentHashes == null)
                return false;
            if (newHashes.Count != currentHashes.Count)
                return false;

            var set1 = new HashSet<string>(newHashes);
            var set2 = new HashSet<string>(currentHashes);

            return set1.SetEquals(set2);
        }

        protected static Expression<Func<Entry, GetEntryResponse>> GetEntryProjection()
        {
            return e => new GetEntryResponse
            {
                Id = e.EntityId,
                DocuType = DocuType.Entry,
                Date = e.Date,
                Mood = e.Mood,
                EncryptedContent = e.EncryptedContent,
                ContentIv = e.ContentIv,
                ContentSalt = e.ContentSalt,
                AttachmentId = e.AttachmentId,
                AttachmentHash = e.AttachmentHash,
                CreatedAt = e.CreatedAt,
                ModifiedAt = e.ModifiedAt,
                DeletedAt = e.DeletedAt,
                PublishedAt = e.PublishedAt,
            };
        }

        protected static Expression<Func<Draft, GetDraftResponse>> GetDraftProjection()
        {
            return d => new GetDraftResponse
            {
                Id = d.EntityId,
                DocuType = DocuType.Draft,
                Date = d.Date,
                Mood = d.Mood,
                EncryptedContent = d.EncryptedContent,
                ContentIv = d.ContentIv,
                ContentSalt = d.ContentSalt,
                AttachmentId = d.AttachmentId,
                AttachmentHash = d.AttachmentHash,
                CreatedAt = d.CreatedAt,
                ModifiedAt = d.ModifiedAt,
            };
        }
    }
}
