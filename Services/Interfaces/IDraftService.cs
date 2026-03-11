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
            NewDraftRequest,
            NewDraftResponse,
            GetDraftResponse,
            UpdateEntryRequest, // need to change for drafts
            UpdateEntryResponse, // need to change for drafts
            DeleteEntryRequest, // need to change for drafts
            DeleteEntryResponse // need to change for drafts
        >
    {
        Task<List<GetDraftResponse>> GetDrafts(Guid userId);
    }
}
