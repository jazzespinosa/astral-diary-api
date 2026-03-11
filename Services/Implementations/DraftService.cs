using System.Net.NetworkInformation;
using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Data;
using AstralDiaryApi.Models.DTOs;
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
            UpdateEntryRequest, // need to change for drafts
            UpdateEntryResponse, // need to change for drafts
            DeleteEntryRequest, // need to change for drafts
            DeleteEntryResponse // need to change for drafts
        >,
            IDraftService
    {
        public DraftService(AppDbContext dbContext, IFileStorageService fileStorageService)
            : base(dbContext, fileStorageService) { }

        public override async Task<NewDraftResponse> Create(
            Guid userId,
            NewDraftRequest newDraftRequest
        )
        {
            var draft = new Draft
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Date = newDraftRequest.Date,
                Title = newDraftRequest.Title,
                Content = newDraftRequest.Content,
                Attachments = new List<Attachment>(),
            };

            await AddAsync(draft);
            await AddAttachmentsAsync(draft, newDraftRequest, draft.EntityId);

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
                Attachments = draft.Attachments,
                CreatedAt = draft.CreatedAt,
                ModifiedAt = draft.ModifiedAt,
            };

            return response;
        }

        public async Task<List<GetDraftResponse>> GetDrafts(Guid userId)
        {
            var response = new List<GetDraftResponse>();
            var drafts = await _dbContext
                .Drafts.Where(d => d.UserId == userId)
                .OrderByDescending(d => d.ModifiedAt)
                .Select(group => new GetDraftResponse
                {
                    Id = group.EntityId,
                    Date = group.Date,
                    Title = group.Title,
                    Content = group.Content,
                    CreatedAt = group.CreatedAt,
                    ModifiedAt = group.ModifiedAt,
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
