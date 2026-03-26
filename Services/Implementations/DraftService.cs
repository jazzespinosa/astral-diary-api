using System.Net.NetworkInformation;
using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Exceptions;
using AstralDiaryApi.Models.DTOs;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Services.Interfaces;
using ImageMagick;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Services.Implementations
{
    public class DraftService
        : BaseEntryService<
            Draft,
            NewDraftRequestProcessed,
            NewDraftResponse,
            GetDraftResponse,
            UpdateDraftRequestProcessed,
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
            NewDraftRequestProcessed newDraftRequest
        )
        {
            var count = await _dbContext.Drafts.CountAsync(d => d.UserId == userId);

            if (count >= 10)
            {
                throw new InvalidOperationException("Maximum number of drafts (10) reached");
            }

            var draft = new Draft
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Date = newDraftRequest.Date,
                Title = newDraftRequest.Title,
                Content = newDraftRequest.Content,
                Mood = newDraftRequest.Mood,
                Attachments = new List<Attachment>(),
            };

            await _dbContext.Drafts.AddAsync(draft);
            await AddAttachmentsAsync(draft, newDraftRequest);
            await _dbContext.SaveChangesAsync();

            var newDraftResponse = new NewDraftResponse
            {
                Id = draft.EntityId,
                Date = draft.Date,
                Title = draft.Title,
            };

            return newDraftResponse;
        }

        public override async Task<GetDraftResponse> Get(Guid userId, string draftId)
        {
            var draft = await FindByIdAsync(userId, draftId);

            if (draft == null)
                throw new ArgumentException("Draft not found");

            var response = new GetDraftResponse
            {
                Id = draft.Id,
                Date = draft.Date,
                Title = draft.Title,
                Content = draft.Content,
                Mood = draft.Mood,
                Attachments = draft.Attachments,
                CreatedAt = draft.CreatedAt,
                ModifiedAt = draft.ModifiedAt,
            };

            return response;
        }

        public override async Task<UpdateDraftResponse> Update(
            Guid userId,
            UpdateDraftRequestProcessed updateDraftRequest
        )
        {
            var draftId = updateDraftRequest.Id;
            var draft = await FindEntityByIdAsync(userId, draftId);

            if (draft == null)
                throw new NotFoundException("Entry not found");

            await UpdateContentsAsync(userId, draft, updateDraftRequest);
            await CompareAndUpdateAttachmentsAsync(userId, draftId, draft, updateDraftRequest);
            await _dbContext.SaveChangesAsync();

            var response = new UpdateDraftResponse
            {
                Id = draftId,
                Date = updateDraftRequest.Date,
                Title = updateDraftRequest.Title,
            };

            return response;
        }

        public async Task<UpdateEntryResponse> PublishDraft(
            Guid userId,
            UpdateDraftRequestProcessed updateDraftRequest
        )
        {
            var draftId = updateDraftRequest.Id;

            var newEntry = new Entry
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Date = updateDraftRequest.Date,
                Title = updateDraftRequest.Title!,
                Content = updateDraftRequest.Content!,
                Mood = updateDraftRequest.Mood,
                Attachments = new List<Attachment>(),
                PublishedAt = DateTime.UtcNow,
            };

            await _entryService.AddDraftPublishToEntryAsync(newEntry, updateDraftRequest); // change function name
            await DeleteDraft(userId, draftId);
            await _dbContext.SaveChangesAsync();
            await _fileStorageService.DeleteAllAttachment(draftId);

            var response = new UpdateEntryResponse
            {
                Id = newEntry.EntityId,
                Date = newEntry.Date,
                Title = newEntry.Title,
            };

            return response;
        }

        public async Task<GetDraftCountResponse> CountDraftsAsync(Guid userId)
        {
            var count = await _dbContext.Drafts.CountAsync(d => d.UserId == userId);
            return new GetDraftCountResponse { Count = count };
        }

        public async Task<bool> DeleteDraft(Guid userId, string draftId)
        {
            var entry = await FindEntityByIdAsync(userId, draftId);

            if (entry == null)
                throw new NotFoundException("Draft not found");

            _dbContext.Drafts.Remove(entry);
            await _dbContext.SaveChangesAsync();
            await _fileStorageService.DeleteAllAttachment(draftId);

            return true;
        }

        public async Task<List<GetDraftResponse>> GetAllDrafts(Guid userId)
        {
            var response = new List<GetDraftResponse>();
            var drafts = await _dbContext
                .Drafts.Where(d => d.UserId == userId)
                .OrderByDescending(d => d.ModifiedAt)
                .Select(d => new GetDraftResponse
                {
                    Id = d.EntityId,
                    Date = d.Date,
                    Title = d.Title,
                    Content = d.Content,
                    Mood = d.Mood,
                    Attachments = d
                        .Attachments.Where(a => a.InternalFileName != null)
                        .Select(a => new AttachmentObjResponse
                        {
                            FilePath = a.FilePath,
                            ThumbnailPath = a.ThumbnailPath,
                            InternalFileName = a.InternalFileName,
                            OriginalFileName = a.OriginalFileName,
                        })
                        .ToList(),
                    CreatedAt = d.CreatedAt,
                    ModifiedAt = d.ModifiedAt,
                })
                .ToListAsync();

            foreach (var draft in drafts)
            {
                response.Add(draft);
            }

            return response;
        }
    }
}
