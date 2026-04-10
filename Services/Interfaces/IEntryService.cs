using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Models.DTOs;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Models.DTOs.Users;
using AstralDiaryApi.Models.Entities;

namespace AstralDiaryApi.Services.Interfaces
{
    public interface IEntryService
        : IBaseEntryService<
            Entry,
            NewEntryRequest,
            NewEntryResponse,
            GetEntryResponse,
            UpdateEntryRequest,
            UpdateEntryResponse
        >
    {
        Task<List<GetEntryResponse>> GetCalendarEntries(Guid userId, DateOnly date);
        Task<List<GetEntryResponse>> GetRecentEntries(Guid userId, int limit);
        Task<PagedResult<GetEntryResponse>> SearchAsync(Guid userId, GetSearchEntryRequest request);
        Task<bool> SoftDeleteEntry(Guid userId, string entryId);

        Task<List<GetEntryIdResponse>> GetEntryIds(Guid userId);
        Task PublishDraftToEntryAsync(
            Entry entry,
            Guid userId,
            UpdateDraftRequest updateDraftRequest
        );
        Task<GetUserInfoResponse> GetUserStatsAsync(
            Guid userId,
            UserInitialDetailsDto userInitialDetailsDto,
            DateOnly currentDate
        );
        Task<List<UserMoodMap>> GetUserMoodMapAsync(Guid userId, int year);
        Task<List<GetEntryResponse>> GetDeletedEntries(Guid userId);
        Task<bool> RestoreEntries(Guid userId, string[] entryIds);
    }
}
