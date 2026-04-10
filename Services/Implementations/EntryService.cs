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
    public class EntryService
        : BaseEntryService<
            Entry,
            NewEntryRequest,
            NewEntryResponse,
            GetEntryResponse,
            UpdateEntryRequest,
            UpdateEntryResponse
        >,
            IEntryService
    {
        public EntryService(AppDbContext dbContext, IFileStorageService fileStorageService)
            : base(dbContext, fileStorageService) { }

        public override async Task<NewEntryResponse> Create(
            Guid userId,
            NewEntryRequest newEntryRequest
        )
        {
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

        public override async Task<GetEntryResponse> Get(Guid userId, string entryId)
        {
            var entry = await FindByIdAsync(userId, entryId, DocuType.Entry);

            if (entry == null)
                throw new NotFoundException("Entry not found");

            return new GetEntryResponse
            {
                Id = entry.Id,
                DocuType = entry.DocuType,
                Date = entry.Date,
                Mood = entry.Mood,
                EncryptedContent = entry.EncryptedContent,
                ContentIv = entry.ContentIv,
                ContentSalt = entry.ContentSalt,
                AttachmentId = entry.AttachmentId,
                AttachmentHash = entry.AttachmentHash,
                CreatedAt = entry.CreatedAt,
                ModifiedAt = entry.ModifiedAt,
                DeletedAt = entry.DeletedAt,
                PublishedAt = entry.PublishedAt,
            };
        }

        public override async Task<UpdateEntryResponse> Update(
            Guid userId,
            UpdateEntryRequest updateEntryRequest
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
            return await _dbContext
                .Entries.AsNoTracking()
                .Where(e =>
                    e.UserId == userId
                    && e.Date.Month >= date.Month - 1
                    && e.Date.Month <= date.Month + 1
                )
                .OrderByDescending(e => e.Date)
                .Select(e => new GetEntryResponse
                {
                    Id = e.EntityId,
                    DocuType = DocuType.Entry,
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
                .ToListAsync();
        }

        public async Task<List<GetEntryResponse>> GetRecentEntries(Guid userId, int limit)
        {
            return await _dbContext
                .Entries.AsNoTracking()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.ModifiedAt)
                .Select(e => new GetEntryResponse
                {
                    Id = e.EntityId,
                    DocuType = DocuType.Entry,
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
                .Take(limit)
                .ToListAsync();
        }

        public async Task<PagedResult<GetEntryResponse>> SearchAsync(
            Guid userId,
            GetSearchEntryRequest getSearchEntriesRequest
        )
        {
            var query = _dbContext.Entries.AsNoTracking().Where(e => e.UserId == userId);

            if (
                getSearchEntriesRequest.DateFilter != null
                && getSearchEntriesRequest.DateFilter?.ToLower() != "any"
                && getSearchEntriesRequest.Date != null
            )
            {
                var dateValue = getSearchEntriesRequest.Date;

                query = getSearchEntriesRequest.DateFilter?.ToLower() switch
                {
                    "exact" => query.Where(e => e.Date == dateValue),
                    "before" => query.Where(e => e.Date < dateValue),
                    "after" => query.Where(e => e.Date > dateValue),
                    _ => query,
                };
            }

            if (getSearchEntriesRequest.Mood >= 0 && getSearchEntriesRequest.Mood <= 5)
            {
                query = query.Where(e => e.Mood == getSearchEntriesRequest.Mood);
            }

            var totalCount = await query.CountAsync();

            query =
                getSearchEntriesRequest.Sort?.ToLower() == "asc"
                    ? query.OrderBy(e => e.Date).ThenBy(e => e.ModifiedAt)
                    : query.OrderByDescending(e => e.Date).ThenByDescending(e => e.ModifiedAt);

            var items = await query
                .Skip((getSearchEntriesRequest.Page - 1) * getSearchEntriesRequest.PageSize)
                .Take(getSearchEntriesRequest.PageSize)
                .Select(e => new GetEntryResponse
                {
                    Id = e.EntityId,
                    DocuType = DocuType.Entry,
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
                .ToListAsync();

            return new PagedResult<GetEntryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = getSearchEntriesRequest.Page,
                PageSize = getSearchEntriesRequest.PageSize,
            };
        }

        public async Task<GetUserInfoResponse> GetUserStatsAsync(
            Guid userId,
            UserInitialDetailsDto userInitialDetailsDto,
            DateOnly currentDate
        )
        {
            var entriesCount = await GetEntriesCountAsync(userId);
            var currentStreak = await GetCurrentStreakAsync(userId, currentDate);
            var firstEntry = await GetFirstEntryAsync(userId);
            var latestEntry = await GetLatestEntryAsync(userId);

            return new GetUserInfoResponse
            {
                Email = userInitialDetailsDto.Email,
                DisplayName = userInitialDetailsDto.DisplayName,
                Avatar = userInitialDetailsDto.Avatar,
                TotalEntries = entriesCount,
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
                .Select(e => new GetEntryResponse
                {
                    Id = e.EntityId,
                    DocuType = DocuType.Entry,
                    Date = e.Date,
                    Mood = e.Mood,
                    EncryptedContent = e.EncryptedContent,
                    ContentIv = e.ContentIv,
                    ContentSalt = e.ContentSalt,
                    AttachmentId = e.AttachmentId,
                    AttachmentHash = e.AttachmentHash,
                    CreatedAt = e.CreatedAt,
                    ModifiedAt = e.ModifiedAt,
                    DeletedAt = e.DeletedAt,
                    PublishedAt = e.PublishedAt,
                })
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
    }
}
