using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Exceptions;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Services.Implementations
{
    public class DraftService
        : BaseEntryService<
            Draft,
            NewDraftRequest,
            NewDraftResponse,
            GetDraftResponse,
            UpdateDraftRequest,
            UpdateDraftResponse
        >,
            IDraftService
    {
        private readonly IEntryService _entryService;

        public DraftService(
            AppDbContext dbContext,
            IFileStorageService fileStorageService,
            IEntryService entryService
        )
            : base(dbContext, fileStorageService)
        {
            _entryService = entryService;
        }

        public override async Task<NewDraftResponse> Create(
            Guid userId,
            NewDraftRequest newDraftRequest
        )
        {
            var count = await _dbContext.Drafts.CountAsync(d => d.UserId == userId);

            if (count >= 10)
            {
                throw new MaxItemsExceededException("Maximum number of drafts (10) reached");
            }

            var draft = new Draft
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Date = newDraftRequest.Date,
                Mood = newDraftRequest.Mood,
                EncryptedContent = newDraftRequest.EncryptedContent,
                ContentIv = newDraftRequest.ContentIv,
                ContentSalt = newDraftRequest.ContentSalt,
            };

            await _dbContext.Drafts.AddAsync(draft);
            await AddAttachmentsAsync(draft, newDraftRequest, userId);
            await _dbContext.SaveChangesAsync();

            return new NewDraftResponse { Id = draft.EntityId };
        }

        public override async Task<GetDraftResponse> Get(Guid userId, string draftId)
        {
            var draft = await FindByIdAsync(userId, draftId, DocuType.Draft);

            if (draft == null)
                throw new NotFoundException("Draft not found");

            return new GetDraftResponse
            {
                Id = draft.Id,
                DocuType = draft.DocuType,
                Date = draft.Date,
                Mood = draft.Mood,
                EncryptedContent = draft.EncryptedContent,
                ContentIv = draft.ContentIv,
                ContentSalt = draft.ContentSalt,
                AttachmentId = draft.AttachmentId,
                AttachmentHash = draft.AttachmentHash,
                CreatedAt = draft.CreatedAt,
                ModifiedAt = draft.ModifiedAt,
            };
        }

        public override async Task<UpdateDraftResponse> Update(
            Guid userId,
            UpdateDraftRequest updateDraftRequest
        )
        {
            var draftId = updateDraftRequest.Id;
            var draft = await FindEntityByIdAsync(userId, draftId);

            if (draft == null)
                throw new NotFoundException("Entry not found");

            await UpdateContentsAsync(userId, draft, updateDraftRequest);
            await UpdateAttachmentsAsync(userId, draftId, draft, updateDraftRequest);
            await _dbContext.SaveChangesAsync();

            return new UpdateDraftResponse { Id = draftId };
        }

        public async Task<UpdateEntryResponse> PublishDraft(
            Guid userId,
            UpdateDraftRequest updateDraftRequest
        )
        {
            var entryDate = updateDraftRequest.Date;
            if (entryDate > DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new ArgumentException("Entry date cannot be in the future.");
            }

            var draftId = updateDraftRequest.Id;
            var draft = await _dbContext.Drafts.FirstOrDefaultAsync(d =>
                d.UserId == userId && d.EntityId == updateDraftRequest.Id
            );

            var newEntry = new Entry
            {
                UserId = userId,
                CreatedAt = draft?.CreatedAt ?? DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Date = updateDraftRequest.Date,
                Mood = updateDraftRequest.Mood,
                EncryptedContent = updateDraftRequest.EncryptedContent,
                ContentIv = updateDraftRequest.ContentIv,
                ContentSalt = updateDraftRequest.ContentSalt,
                PublishedAt = DateTime.UtcNow,
            };

            await _entryService.PublishDraftToEntryAsync(newEntry, userId, updateDraftRequest);
            await DeleteDraft(userId, draftId);
            await _dbContext.SaveChangesAsync();

            await _fileStorageService.DeleteSavedAttachment(draftId, userId);

            return new UpdateEntryResponse { Id = newEntry.EntityId };
        }

        public async Task<GetDraftCountResponse> CountDraftsAsync(Guid userId)
        {
            var count = await _dbContext.Drafts.CountAsync(d => d.UserId == userId);
            return new GetDraftCountResponse { Count = count };
        }

        public async Task<bool> DeleteDraft(Guid userId, string draftId)
        {
            var draft = await FindEntityByIdAsync(userId, draftId);

            if (draft == null)
                throw new NotFoundException("Draft not found");

            _dbContext.Drafts.Remove(draft);
            await _dbContext.SaveChangesAsync();

            await _fileStorageService.DeleteSavedAttachment(draftId, userId);

            return true;
        }

        public async Task<List<GetDraftResponse>> GetAllDrafts(Guid userId)
        {
            return await _dbContext
                .Drafts.Where(d => d.UserId == userId)
                .OrderByDescending(d => d.ModifiedAt)
                .Select(d => new GetDraftResponse
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
                })
                .ToListAsync();
        }
    }
}
