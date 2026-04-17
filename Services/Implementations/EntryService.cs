using System.Data;
using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Exceptions;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Models.DTOs.Users;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Services.Implementations
{
    public class EntryService : BaseEntryService<Entry>, IEntryService
    {
        public EntryService(AppDbContext dbContext, IFileStorageService fileStorageService)
            : base(dbContext, fileStorageService) { }

        public override async Task<IResponseDto> Create(Guid userId, IRequestDto newEntryRequest)
        {
            var createdEntriesToday = await CountNewEntriesToday(userId);

            if (createdEntriesToday >= 3)
            {
                throw new MaxItemsExceededException(
                    "Maximum number of new entries per day (3) reached"
                );
            }

            var entry = new Entry
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Date = newEntryRequest.Date,
                Mood = newEntryRequest.Mood,
                EncryptedContent = newEntryRequest.EncryptedContent,
                ContentIv = newEntryRequest.ContentIv,
                ContentSalt = newEntryRequest.ContentSalt,
            };

            await _dbContext.Entries.AddAsync(entry);
            await AddAttachmentsAsync(entry, newEntryRequest, userId);
            await _dbContext.SaveChangesAsync();

            return new NewEntryResponse { Id = entry.EntityId };
        }

        public override async Task<IGetResponse> Get(Guid userId, string entryId)
        {
            var entry = await FindByIdAsync(userId, entryId, DocuType.Entry);

            if (entry == null)
                throw new NotFoundException("Entry not found");

            return entry;
        }

        public override async Task<IUpdateResponse> Update(
            Guid userId,
            IUpdateRequest updateEntryRequest
        )
        {
            var entryId = updateEntryRequest.Id;
            var entry = await FindEntityByIdAsync(userId, entryId);

            if (entry == null)
                throw new NotFoundException("Entry not found");

            await UpdateContentsAsync(userId, entry, updateEntryRequest);
            await UpdateAttachmentsAsync(userId, entryId, entry, updateEntryRequest);
            await _dbContext.SaveChangesAsync();

            return new UpdateEntryResponse { Id = entry.EntityId };
        }

        public async Task PublishDraftToEntryAsync(
            Entry entry,
            Guid userId,
            UpdateDraftRequest updateDraftRequest
        )
        {
            await _dbContext.Entries.AddAsync(entry);
            await AddAttachmentsAsync(entry, updateDraftRequest, userId);
        }

        public async Task<bool> SoftDeleteEntry(Guid userId, string entryId)
        {
            var entry = await FindEntityByIdAsync(userId, entryId);

            if (entry == null)
                throw new NotFoundException("Entry not found");

            entry.IsDeleted = true;
            entry.ModifiedAt = DateTime.UtcNow;
            entry.DeletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<int> CountNewEntriesToday(Guid userId)
        {
            var (startUtc, endUtc) = GetLocalDayRangeUtc();

            return await _dbContext
                .Entries.AsNoTracking()
                .IgnoreQueryFilters()
                .CountAsync(e =>
                    e.UserId == userId && e.CreatedAt >= startUtc && e.CreatedAt <= endUtc
                );
        }

        public async Task<List<GetEntryIdResponse>> GetEntryIds(Guid userId)
        {
            var response = new List<GetEntryIdResponse>();

            var entryIds = await _dbContext
                .Entries.AsNoTracking()
                .Where(e => e.UserId == userId)
                .Select(e => e.EntityId)
                .ToListAsync();

            for (int i = 0; i < entryIds.Count; i++)
            {
                response.Add(new GetEntryIdResponse { Id = (i + 1), EntryId = entryIds[i] });
            }

            return response;
        }

        public async Task<List<GetEntryResponse>> GetCalendarEntries(Guid userId, DateOnly date)
        {
            var startDate = date.AddMonths(-1);
            var endDate = date.AddMonths(1).AddDays(-1);

            return await _dbContext
                .Entries.AsNoTracking()
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .Select(GetEntryProjection())
                .ToListAsync();
        }

        public async Task<List<GetEntryResponse>> GetRecentEntries(Guid userId, int limit)
        {
            return await _dbContext
                .Entries.AsNoTracking()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.ModifiedAt)
                .Select(GetEntryProjection())
                .Take(limit)
                .ToListAsync();
        }

        public async Task<PagedResult> SearchAsync(
            Guid userId,
            GetSearchEntryRequest getSearchEntriesRequest
        )
        {
            var query = _dbContext.Entries.AsNoTracking().Where(e => e.UserId == userId);

            if (
                getSearchEntriesRequest.DateFilter != null
                && getSearchEntriesRequest.DateFilter != DateTypeFilter.any
                && getSearchEntriesRequest.Date != null
            )
            {
                var dateValue = getSearchEntriesRequest.Date;

                query = getSearchEntriesRequest.DateFilter switch
                {
                    DateTypeFilter.exact => query.Where(e => e.Date == dateValue),
                    DateTypeFilter.before => query.Where(e => e.Date < dateValue),
                    DateTypeFilter.after => query.Where(e => e.Date > dateValue),
                    _ => query,
                };
            }

            if (getSearchEntriesRequest.Mood >= 0 && getSearchEntriesRequest.Mood <= 5)
            {
                query = query.Where(e => e.Mood == getSearchEntriesRequest.Mood);
            }

            query =
                getSearchEntriesRequest.Sort == SortType.asc
                    ? query.OrderBy(e => e.Date).ThenBy(e => e.ModifiedAt)
                    : query.OrderByDescending(e => e.Date).ThenByDescending(e => e.ModifiedAt);

            var items = await query.Select(GetEntryProjection()).ToListAsync();

            return new PagedResult { Items = items };
        }

        public async Task<GetUserInfoResponse> GetUserStatsAsync(
            Guid userId,
            UserInitialDetailsDto userInitialDetailsDto,
            DateOnly currentDate
        )
        {
            var dailyEntriesCount = await CountNewEntriesToday(userId);
            var totalEntriesCount = await GetEntriesCountAsync(userId);
            var currentStreak = await GetCurrentStreakAsync(userId, currentDate);
            var firstEntry = await GetFirstEntryAsync(userId);
            var latestEntry = await GetLatestEntryAsync(userId);

            return new GetUserInfoResponse
            {
                Email = userInitialDetailsDto.Email,
                DisplayName = userInitialDetailsDto.DisplayName,
                Avatar = userInitialDetailsDto.Avatar,
                DailyEntries = dailyEntriesCount,
                TotalEntries = totalEntriesCount,
                FirstEntryId = firstEntry?.EntryId,
                FirstEntryDate = firstEntry?.Date,
                LatestEntryId = latestEntry?.EntryId,
                LatestEntryDate = latestEntry?.Date,
                CurrentStreak = currentStreak,
            };
        }

        private async Task<int> GetEntriesCountAsync(Guid userId)
        {
            return await _dbContext.Entries.AsNoTracking().CountAsync(e => e.UserId == userId);
        }

        private async Task<Entry?> GetFirstEntryAsync(Guid userId)
        {
            return await _dbContext
                .Entries.AsNoTracking()
                .OrderBy(e => e.Date)
                .ThenBy(e => e.ModifiedAt)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }

        private async Task<Entry?> GetLatestEntryAsync(Guid userId)
        {
            return await _dbContext
                .Entries.AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.ModifiedAt)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }

        private async Task<int> GetCurrentStreakAsync(Guid userId, DateOnly currentUserDate)
        {
            var entries = await _dbContext
                .Entries.AsNoTracking()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Select(e => e.Date)
                .ToListAsync();

            if (entries.Count == 0)
                return 0;

            int streak = 1;
            var today = currentUserDate;
            var currentDate = entries[0];

            if (currentDate != today && currentDate != today.AddDays(-1))
                return 0;

            for (int i = 1; i < entries.Count; i++)
            {
                if (entries[i] == currentDate.AddDays(-1))
                {
                    streak++;
                    currentDate = entries[i];
                }
                else if (entries[i] == currentDate)
                {
                    continue;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        public async Task<List<UserMoodMap>> GetUserMoodMapAsync(Guid userId, int year)
        {
            return await _dbContext
                .Entries.AsNoTracking()
                .Where(e => e.UserId == userId && e.Date.Year == year)
                .Select(e => new UserMoodMap { Date = e.Date, Mood = e.Mood })
                .ToListAsync();
        }

        public async Task<List<GetEntryResponse>> GetDeletedEntries(Guid userId)
        {
            return await _dbContext
                .Entries.AsNoTracking()
                .IgnoreQueryFilters()
                .Where(e => e.UserId == userId && e.IsDeleted == true && e.DeletedAt != null)
                .OrderByDescending(e => e.DeletedAt)
                .ThenByDescending(e => e.Date)
                .ThenByDescending(e => e.ModifiedAt)
                .Select(GetEntryProjection())
                .ToListAsync();
        }

        public async Task<bool> RestoreEntries(Guid userId, string[] entryIds)
        {
            var entries = await _dbContext
                .Entries.IgnoreQueryFilters()
                .Where(e =>
                    e.UserId == userId
                    && entryIds.Contains(e.EntityId)
                    && e.IsDeleted == true
                    && e.DeletedAt != null
                )
                .ToListAsync();

            if (entries.Count == 0)
                throw new NotFoundException("No Entries found");

            foreach (var entryId in entries)
            {
                entryId.IsDeleted = false;
                entryId.DeletedAt = null;
                entryId.ModifiedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            return true;
        }

        // UTC+8 (Manila Time)
        private (DateTime startUtc, DateTime endUtc) GetLocalDayRangeUtc()
        {
            var timezone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
            var nowUtc = DateTime.UtcNow;
            var nowPh = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone);

            var startPh = nowPh.Date;
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startPh, timezone);

            return (startUtc, startUtc.AddDays(1));
        }
    }
}
