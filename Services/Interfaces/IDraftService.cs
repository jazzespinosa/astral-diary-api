using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Models.DTOs;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Models.Entities;

namespace AstralDiaryApi.Services.Interfaces
{
    public interface IDraftService
        : IBaseEntryService<
            Draft,
            NewDraftRequestProcessed,
            NewDraftResponse,
            GetDraftResponse,
            UpdateDraftRequestProcessed,
            UpdateDraftResponse
        >
    {
        Task<List<GetDraftResponse>> GetAllDrafts(Guid userId);
        Task<GetDraftCountResponse> CountDraftsAsync(Guid userId);
        Task<bool> DeleteDraft(Guid userId, string draftId);
        Task<UpdateEntryResponse> PublishDraft(
            Guid userId,
            UpdateDraftRequestProcessed updateDraftRequest
        );
    }
}
