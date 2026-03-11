using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Models.DTOs;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
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
            UpdateEntryResponse,
            DeleteEntryRequest,
            DeleteEntryResponse
        >
    {
        Task<List<GetEntryResponse>> GetCalendarEntries(Guid userId, DateOnly date);
        Task<List<GetEntryResponse>> GetRecentEntries(Guid userId, int limit);
        Task<PagedResult<GetSearchEntryResponse>> SearchAsync(
            Guid userId,
            GetSearchEntryRequest request
        );
    }
}
