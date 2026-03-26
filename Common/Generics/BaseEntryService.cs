using System.Net;
using System.Net.Mail;
using AstralDiaryApi.Common.Helpers;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Models.DTOs;
using AstralDiaryApi.Models.DTOs.Attachments;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Models.Enums;
using AstralDiaryApi.Services.Implementations;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Attachment = AstralDiaryApi.Models.Entities.Attachment;

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
        where TEntity : class, IAttachmentSource, IEntityIdSource
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

        protected async Task<IGetResponse?> FindByIdAsync(Guid userId, string entityId)
        {
            return await _dbContext
                .Set<TEntity>()
                .Where(e => e.EntityId == entityId && e.UserId == userId)
                .Select(e => new GetEntryResponse
                {
                    Id = e.EntityId,
                    Date = e.Date,
                    Title = e.Title ?? "",
                    Content = e.Content ?? "",
                    Mood = e.Mood,
                    Attachments = e
                        .Attachments.Where(a => a.InternalFileName != null)
                        .Select(a => new AttachmentObjResponse
                        {
                            FilePath = a.FilePath,
                            ThumbnailPath = a.ThumbnailPath,
                            InternalFileName = a.InternalFileName,
                            OriginalFileName = a.OriginalFileName,
                        })
                        .ToList(),
                    CreatedAt = e.CreatedAt,
                    ModifiedAt = e.ModifiedAt,
                })
                .FirstOrDefaultAsync();
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
            IRequestDto<AttachmentObjRequest> updateEntryRequest
        )
        {
            if (entity == null)
                throw new Exception("Entry does not exist.");

            entity.Date = updateEntryRequest.Date;
            entity.Title = updateEntryRequest.Title;
            entity.Content = updateEntryRequest.Content;
            entity.Mood = updateEntryRequest.Mood;
            entity.ModifiedAt = DateTime.UtcNow;
        }

        protected async Task CompareAndUpdateAttachmentsAsync(
            Guid userId,
            string entityId,
            TEntity entity,
            IRequestDto<AttachmentObjRequest> updateEntryRequest
        )
        {
            var currentAttachments = await _dbContext
                .Set<TEntity>()
                .Where(e => e.UserId == userId && e.EntityId == entity.EntityId)
                .SelectMany(e => e.Attachments)
                .Where(a => a.InternalFileName != null)
                .Select(a => new AttachmentComparisonDto
                {
                    ContentHash = a.ContentHash,
                    OriginalFileName = a.OriginalFileName,
                    InternalFileName = a.InternalFileName,
                })
                .ToListAsync();

            var newAttachments = updateEntryRequest
                .Attachments?.Select(a => new AttachmentComparisonDto
                {
                    ContentHash = a.ContentHash,
                    OriginalFileName = a.File.FileName,
                    InternalFileName = "",
                })
                .ToList();

            if (newAttachments == null || newAttachments.Count == 0)
            {
                if (entityId.StartsWith("draft-"))
                {
                    await _dbContext
                        .Attachments.Where(a => a.DraftId == entityId)
                        .ExecuteDeleteAsync();
                }
                else if (entityId.StartsWith("entry-"))
                {
                    await _dbContext
                        .Attachments.Where(a => a.EntryId == entityId)
                        .ExecuteDeleteAsync();
                }

                await _dbContext.SaveChangesAsync();
                await _fileStorageService.DeleteAllAttachment(entityId);
                return;
            }

            var currentSet = currentAttachments.ToHashSet();
            var newSet = newAttachments.ToHashSet();

            var attachmentsToDelete = currentSet
                .ExceptBy(newSet.Select(n => n.ContentHash), c => c.ContentHash)
                .ToList();
            var attachmentsToAdd = newSet
                .ExceptBy(currentSet.Select(c => c.ContentHash), n => n.ContentHash)
                .ToList();
            var attachmentsToKeep = currentSet
                .IntersectBy(newSet.Select(n => n.ContentHash), c => c.ContentHash)
                .ToList();

            if (attachmentsToDelete.Count > 0)
            {
                var hashesToDelete = attachmentsToDelete.Select(a => a.ContentHash).ToList();

                await _dbContext
                    .Attachments.Where(a => hashesToDelete.Contains(a.ContentHash))
                    .ExecuteDeleteAsync();
            }

            foreach (var addAttachment in attachmentsToAdd)
            {
                var file = updateEntryRequest
                    .Attachments?.Where(a =>
                        a.ContentHash == addAttachment.ContentHash
                        && a.File.FileName == addAttachment.OriginalFileName
                    )
                    .FirstOrDefault();
                var path = await _fileStorageService.SaveAttachment(file!.File, entityId);
                var attachment = new Attachment
                {
                    InternalFileName = path.InternalFileName,
                    OriginalFileName = file.File.FileName,
                    FilePath = path.FilePath,
                    ThumbnailPath = path.ThumbnailPath,
                    ContentHash = file.ContentHash,
                    CreatedAt = DateTime.UtcNow,
                };

                entity.LinkAttachment(attachment);
                await _dbContext.Attachments.AddAsync(attachment);
            }

            await RenameOriginalFileNamesAsync(attachmentsToKeep, updateEntryRequest);
            await _dbContext.SaveChangesAsync();
            await _fileStorageService.DeleteAttachmentAndThumbnail(attachmentsToDelete, entityId);
        }

        protected async Task AddAttachmentsAsync(
            TEntity entity,
            IRequestDto<AttachmentObjRequest> newRequest
        )
        {
            if (newRequest.Attachments != null && newRequest.Attachments.Count > 0)
            {
                foreach (var file in newRequest.Attachments)
                {
                    var path = await _fileStorageService.SaveAttachment(file.File, entity.EntityId);

                    var attachment = new Attachment
                    {
                        InternalFileName = path.InternalFileName,
                        OriginalFileName = file.File.FileName,
                        FilePath = path.FilePath,
                        ThumbnailPath = path.ThumbnailPath,
                        ContentHash = file.ContentHash,
                        CreatedAt = DateTime.UtcNow,
                    };

                    entity.LinkAttachment(attachment);
                    await _dbContext.Attachments.AddAsync(attachment);
                }
            }
        }

        private async Task RenameOriginalFileNamesAsync(
            List<AttachmentComparisonDto> attachmentsToKeep,
            IRequestDto<AttachmentObjRequest> updateEntryRequest
        )
        {
            if (attachmentsToKeep == null || !attachmentsToKeep.Any())
                return;

            var hashes = attachmentsToKeep.Select(a => a.ContentHash).ToList();

            var originalAttachments = await _dbContext
                .Attachments.Where(a => hashes.Contains(a.ContentHash))
                .ToListAsync();

            var newAttachmentsDict = updateEntryRequest
                .Attachments?.Where(a => a.ContentHash != null)
                .ToDictionary(a => a.ContentHash);

            if (newAttachmentsDict == null)
                return;

            foreach (var original in originalAttachments)
            {
                if (!newAttachmentsDict.TryGetValue(original.ContentHash, out var newAttachment))
                    continue;

                var newFileName = newAttachment.File?.FileName;

                if (
                    string.IsNullOrEmpty(original.OriginalFileName)
                    || string.IsNullOrEmpty(newFileName)
                )
                    continue;

                if (
                    !string.Equals(original.OriginalFileName, newFileName, StringComparison.Ordinal)
                )
                {
                    original.OriginalFileName = newFileName;
                }
            }
        }
    }
}
