using System.CodeDom;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Exceptions;
using AstralDiaryApi.Models.DTOs;
using AstralDiaryApi.Models.DTOs.Attachments;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Services.Implementations;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Common.Generics
{
    public abstract class BaseEntryService<
        TEntity,
        TNewRequest,
        TNewResponse,
        TGetResponse,
        TUpdateRequest,
        TUpdateResponse
    >
        where TEntity : class, IEntityIdSource
    {
        protected readonly AppDbContext _dbContext;
        protected readonly IFileStorageService _fileStorageService;

        public BaseEntryService(AppDbContext dbContext, IFileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
        }

        public abstract Task<TNewResponse> Create(Guid userId, TNewRequest newRequest);

        public abstract Task<TGetResponse> Get(Guid userId, string entityId);

        public abstract Task<TUpdateResponse> Update(Guid userId, TUpdateRequest updateRequest);

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
                        .Select(e => new GetResponse
                        {
                            Id = e.EntityId,
                            DocuType = docuType,
                            Date = e.Date,
                            Mood = e.Mood,
                            EncryptedContent = e.EncryptedContent,
                            ContentIv = e.ContentIv,
                            ContentSalt = e.ContentSalt,
                            AttachmentId = e.AttachmentId,
                            AttachmentHash = e.AttachmentHash,
                            CreatedAt = e.CreatedAt,
                            ModifiedAt = e.ModifiedAt,
                            PublishedAt = e.PublishedAt,
                        })
                        .FirstOrDefaultAsync();
                case DocuType.Draft:
                    return await _dbContext
                        .Drafts.Where(e => e.EntityId == entityId && e.UserId == userId)
                        .Select(e => new GetResponse
                        {
                            Id = e.EntityId,
                            DocuType = docuType,
                            Date = e.Date,
                            Mood = e.Mood,
                            EncryptedContent = e.EncryptedContent,
                            ContentIv = e.ContentIv,
                            ContentSalt = e.ContentSalt,
                            AttachmentId = e.AttachmentId,
                            AttachmentHash = e.AttachmentHash,
                            CreatedAt = e.CreatedAt,
                            ModifiedAt = e.ModifiedAt,
                        })
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

        protected async Task UpdateContentsAsync(
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
    }
}
