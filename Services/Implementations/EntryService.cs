using System.Net.NetworkInformation;
using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Models.DTOs;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Models.Enums;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace AstralDiaryApi.Services.Implementations
{
    public class EntryService
        : BaseEntryService<
            Entry,
            NewEntryRequest,
            NewEntryResponse,
            GetEntryResponse,
            UpdateEntryRequest,
            UpdateEntryResponse,
            DeleteEntryRequest,
            DeleteEntryResponse
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
                Title = newEntryRequest.Title,
                Content = newEntryRequest.Content,
                Attachments = new List<Attachment>(),
            };

            await AddAsync(entry);
            await AddAttachmentsAsync(entry, newEntryRequest, entry.EntityId);

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
                Attachments = entry.Attachments,
                CreatedAt = entry.CreatedAt,
                ModifiedAt = entry.ModifiedAt,
            };

            return response;
        }

        public async Task<List<GetEntryResponse>> GetCalendarEntries(Guid userId, DateOnly date)
        {
            var response = new List<GetEntryResponse>();
            var entries = await _dbContext
                .Entries.Where(e =>
                    e.UserId == userId
                    && e.Date.Month >= date.Month - 1
                    && e.Date.Month <= date.Month + 1
                )
                .OrderByDescending(e => e.Date)
                .Select(group => new GetEntryResponse
                {
                    Id = group.EntityId,
                    Date = group.Date,
                    Title = group.Title,
                    Content = group.Content,
                    CreatedAt = group.CreatedAt,
                    ModifiedAt = group.ModifiedAt,
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
                .Entries.Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.ModifiedAt)
                .Select(e => new GetEntryResponse
                {
                    Id = e.EntityId,
                    Date = e.Date,
                    Title = e.Title,
                    Content = e.Content,
                    Attachments = e
                        .Attachments.Where(a => a.ThumbnailPath != null)
                        .Select(a => new AttachmentObjResponse
                        {
                            FilePath = a.FilePath,
                            ThumbnailPath = a.ThumbnailPath,
                            InternalFileName = a.InternalFileName,
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

        //public async Task<List<GetEntriesResponse>> GetSearchEntries(Guid userId, string query) { }

        public async Task<PagedResult<GetSearchEntryResponse>> SearchAsync(
            Guid userId,
            GetSearchEntryRequest getSearchEntriesRequest
        )
        {
            if (string.IsNullOrWhiteSpace(getSearchEntriesRequest.SearchTerm))
                return await GetAllPagedAsync(userId, getSearchEntriesRequest);

            var term = getSearchEntriesRequest.SearchTerm.Trim();
            var booleanTerm = string.Join(
                " ",
                term.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => $"+{w}*")
            );

            var totalCount = await CountFullTextAsync(userId, booleanTerm);

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
                .Include(e => e.Attachments)
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

            query =
                getSearchEntriesRequest.Sort?.ToLower() == "asc"
                    ? query.OrderBy(e => e.Date)
                    : query.OrderByDescending(e => e.Date);

            var items = await query
                .Skip(offset)
                .Take(getSearchEntriesRequest.PageSize)
                .Select(e => new GetSearchEntryResponse
                {
                    EntryId = e.EntityId,
                    Date = e.Date,
                    Title = e.Title,
                    Content = e.Content,
                    AttachmentsThumbnails = e
                        .Attachments.Where(a => a.ThumbnailPath != null)
                        .Select(a => a.ThumbnailPath!)
                        .ToList(),
                    CreatedAt = e.CreatedAt,
                    ModifiedAt = e.ModifiedAt,
                })
                .ToListAsync();

            return new PagedResult<GetSearchEntryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = getSearchEntriesRequest.Page,
                PageSize = getSearchEntriesRequest.PageSize,
            };
        }

        private async Task<int> CountFullTextAsync(Guid userId, string booleanTerm)
        {
            await _dbContext.Database.OpenConnectionAsync();
            try
            {
                using var cmd = _dbContext.Database.GetDbConnection().CreateCommand();
                cmd.CommandText =
                    @"
            SELECT COUNT(*) FROM entries
            WHERE user_id = @userId
              AND MATCH(title, content) AGAINST(@term IN BOOLEAN MODE)";
                var userParam = cmd.CreateParameter();
                userParam.ParameterName = "@userId";
                userParam.Value = userId;
                cmd.Parameters.Add(userParam);
                var termParam = cmd.CreateParameter();
                termParam.ParameterName = "@term";
                termParam.Value = booleanTerm;
                cmd.Parameters.Add(termParam);
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            finally
            {
                await _dbContext.Database.CloseConnectionAsync();
            }
        }

        private async Task<PagedResult<GetSearchEntryResponse>> GetAllPagedAsync(
            Guid userId,
            GetSearchEntryRequest getSearchEntriesRequest
        )
        {
            var query = _dbContext.Entries.AsNoTracking().Where(e => e.UserId == userId);

            var totalCount = await query.CountAsync();

            query =
                getSearchEntriesRequest.Sort?.ToLower() == "asc"
                    ? query.OrderBy(e => e.Date)
                    : query.OrderByDescending(e => e.Date);

            var items = await query
                .Skip((getSearchEntriesRequest.Page - 1) * getSearchEntriesRequest.PageSize)
                .Take(getSearchEntriesRequest.PageSize)
                .Select(e => new GetSearchEntryResponse
                {
                    EntryId = e.EntityId,
                    Date = e.Date,
                    Title = e.Title,
                    Content = e.Content,
                    AttachmentsThumbnails = e
                        .Attachments.Where(a => a.ThumbnailPath != null)
                        .Select(a => a.ThumbnailPath!)
                        .ToList(),
                    CreatedAt = e.CreatedAt,
                    ModifiedAt = e.ModifiedAt,
                })
                .ToListAsync();

            return new PagedResult<GetSearchEntryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = getSearchEntriesRequest.Page,
                PageSize = getSearchEntriesRequest.PageSize,
            };
        }
    }
}
