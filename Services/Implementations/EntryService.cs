using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Common.Helpers;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Exceptions;
using AstralDiaryApi.Models.DTOs;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Models.DTOs.Users;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Models.Enums;
using AstralDiaryApi.Services.Interfaces;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Attachment = AstralDiaryApi.Models.Entities.Attachment;

namespace AstralDiaryApi.Services.Implementations
{
    public class EntryService
        : BaseEntryService<
            Entry,
            NewEntryRequestProcessed,
            NewEntryResponse,
            GetEntryResponse,
            UpdateEntryRequestProcessed,
            UpdateEntryResponse
        >,
            IEntryService
    {
        public EntryService(AppDbContext dbContext, IFileStorageService fileStorageService)
            : base(dbContext, fileStorageService) { }

        public override async Task<NewEntryResponse> Create(
            Guid userId,
            NewEntryRequestProcessed newEntryRequest
        )
        {
            var entry = new Entry
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                Date = newEntryRequest.Date,
                Title = newEntryRequest.Title,
                Content = newEntryRequest.Content,
                Mood = newEntryRequest.Mood,
                Attachments = new List<Attachment>(),
            };

            await _dbContext.Entries.AddAsync(entry);
            await AddAttachmentsAsync(entry, newEntryRequest);
            await _dbContext.SaveChangesAsync();

            var newEntryResponse = new NewEntryResponse
            {
                Id = entry.EntityId,
                Date = entry.Date,
                Title = entry.Title,
            };

            return newEntryResponse;
        }

        public override async Task<GetEntryResponse> Get(Guid userId, string entryId)
        {
            var entry = await FindByIdAsync(userId, entryId);

            if (entry == null)
                throw new ArgumentException("Entry not found");

            var response = new GetEntryResponse
            {
                Id = entry.Id,
                Date = entry.Date,
                Title = entry.Title ?? "",
                Content = entry.Content ?? "",
                Mood = entry.Mood,
                Attachments = entry.Attachments,
                CreatedAt = entry.CreatedAt,
                ModifiedAt = entry.ModifiedAt,
            };

            return response;
        }

        public override async Task<UpdateEntryResponse> Update(
            Guid userId,
            UpdateEntryRequestProcessed updateEntryRequest
        )
        {
            var entryId = updateEntryRequest.Id;
            var entry = await FindEntityByIdAsync(userId, entryId);

            if (entry == null)
                throw new NotFoundException("Entry not found");

            await UpdateContentsAsync(userId, entry, updateEntryRequest);
            await CompareAndUpdateAttachmentsAsync(userId, entryId, entry, updateEntryRequest);
            await _dbContext.SaveChangesAsync();

            var response = new UpdateEntryResponse
            {
                Id = entryId,
                Date = updateEntryRequest.Date,
                Title = updateEntryRequest.Title,
            };

            return response;
        }

        public async Task AddDraftPublishToEntryAsync(
            Entry entry,
            UpdateDraftRequestProcessed updateDraftRequest
        )
        {
            await _dbContext.Entries.AddAsync(entry);
            await AddAttachmentsAsync(entry, updateDraftRequest);
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

        public async Task<List<GetEntryResponse>> GetCalendarEntries(Guid userId, DateOnly date)
        {
            var response = new List<GetEntryResponse>();
            var entries = await _dbContext
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
                    Date = e.Date,
                    Title = e.Title,
                    Content = e.Content,
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
                .ToListAsync();

            foreach (var entry in entries)
            {
                response.Add(entry);
            }

            return response;
        }

        public async Task<List<GetEntryResponse>> GetRecentEntries(Guid userId, int limit)
        {
            var response = new List<GetEntryResponse>();
            var entries = await _dbContext
                .Entries.AsNoTracking()
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.ModifiedAt)
                .Select(e => new GetEntryResponse
                {
                    Id = e.EntityId,
                    Date = e.Date,
                    Title = e.Title,
                    Content = e.Content,
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
                .Take(limit)
                .ToListAsync();

            foreach (var entry in entries)
            {
                response.Add(entry);
            }

            return response;
        }

        public async Task<PagedResult<GetEntryResponse>> SearchAsync(
            Guid userId,
            GetSearchEntryRequest getSearchEntriesRequest
        )
        {
            if (string.IsNullOrWhiteSpace(getSearchEntriesRequest.SearchTerm))
                return await GetBlankSearchPagedAsync(userId, getSearchEntriesRequest);

            var term = getSearchEntriesRequest.SearchTerm.Trim();
            var booleanTerm = string.Join(
                " ",
                term.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => $"+{w}*")
            );

            //var totalCount = await CountFullTextAsync(userId, booleanTerm);

            var offset = (getSearchEntriesRequest.Page - 1) * getSearchEntriesRequest.PageSize;

            var sql =
                $@"
                SELECT *, 
                       MATCH(title, content) AGAINST(@term IN BOOLEAN MODE) AS Score
                FROM entries
                WHERE user_id = @userId
                  AND MATCH(title, content) AGAINST(@term IN BOOLEAN MODE)";

            var query = _dbContext
                .Entries.FromSqlRaw(
                    sql,
                    new MySqlParameter("@term", booleanTerm),
                    new MySqlParameter("@userId", userId)
                )
                .AsNoTracking();

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

            if (getSearchEntriesRequest.Mood >= 1 && getSearchEntriesRequest.Mood <= 5)
            {
                query = query.Where(e => e.Mood == getSearchEntriesRequest.Mood);
            }

            var totalCount = await query.CountAsync();

            query =
                getSearchEntriesRequest.Sort?.ToLower() == "asc"
                    ? query.OrderBy(e => e.Date).ThenBy(e => e.ModifiedAt)
                    : query.OrderByDescending(e => e.Date).ThenByDescending(e => e.ModifiedAt);

            var items = await query
                .Skip(offset)
                .Take(getSearchEntriesRequest.PageSize)
                .Select(e => new GetEntryResponse
                {
                    Id = e.EntityId,
                    Date = e.Date,
                    Title = e.Title,
                    Content = e.Content,
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
                .ToListAsync();

            return new PagedResult<GetEntryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = getSearchEntriesRequest.Page,
                PageSize = getSearchEntriesRequest.PageSize,
            };
        }

        //private async Task<int> CountFullTextAsync(Guid userId, string booleanTerm)
        //{
        //    await _dbContext.Database.OpenConnectionAsync();
        //    try
        //    {
        //        using var cmd = _dbContext.Database.GetDbConnection().CreateCommand();
        //        cmd.CommandText =
        //            @"
        //    SELECT COUNT(*) FROM entries
        //    WHERE user_id = @userId
        //      AND MATCH(title, content) AGAINST(@term IN BOOLEAN MODE)";
        //        var userParam = cmd.CreateParameter();
        //        userParam.ParameterName = "@userId";
        //        userParam.Value = userId;
        //        cmd.Parameters.Add(userParam);
        //        var termParam = cmd.CreateParameter();
        //        termParam.ParameterName = "@term";
        //        termParam.Value = booleanTerm;
        //        cmd.Parameters.Add(termParam);
        //        var result = await cmd.ExecuteScalarAsync();
        //        return Convert.ToInt32(result);
        //    }
        //    finally
        //    {
        //        await _dbContext.Database.CloseConnectionAsync();
        //    }
        //}

        private async Task<PagedResult<GetEntryResponse>> GetBlankSearchPagedAsync(
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

            if (getSearchEntriesRequest.Mood >= 1 && getSearchEntriesRequest.Mood <= 5)
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
                    Date = e.Date,
                    Title = e.Title,
                    Content = e.Content,
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

            var response = new GetUserInfoResponse
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

            return response;
        }

        private async Task<int> GetEntriesCountAsync(Guid userId)
        {
            return await _dbContext.Entries.AsNoTracking().CountAsync(e => e.UserId == userId);
        }

        private async Task<Entry?> GetFirstEntryAsync(Guid userId)
        {
            var entry = await _dbContext
                .Entries.AsNoTracking()
                .OrderBy(e => e.Date)
                .ThenBy(e => e.ModifiedAt)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            return entry;
        }

        private async Task<Entry?> GetLatestEntryAsync(Guid userId)
        {
            var entry = await _dbContext
                .Entries.AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.ModifiedAt)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            return entry;
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

        public async Task<List<UserMoodMap>> GetUserMoodMapAsync(Guid userId)
        {
            return await _dbContext
                .Entries.AsNoTracking()
                .Where(e => e.UserId == userId)
                .Select(e => new UserMoodMap { Date = e.Date, Mood = e.Mood })
                .ToListAsync();
        }
    }
}
