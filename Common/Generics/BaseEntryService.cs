using System.Net;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Data;
using AstralDiaryApi.Models.DTOs;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Models.Enums;
using AstralDiaryApi.Services.Implementations;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Common.Generics
{
    public abstract class BaseEntryService<
        TEntity,
        TNewRequest,
        TNewResponse,
        TGetResponse,
        TUpdateRequest,
        TUpdateResponse,
        TDeleteRequest,
        TDeleteResponse
    >
        where TEntity : class, IAttachmentSource, IEntityIdSource
        where TNewRequest : INewRequest
        where TNewResponse : INewResponse
        where TGetResponse : IGetResponse
        where TUpdateRequest : UpdateEntryRequest
        where TUpdateResponse : UpdateEntryResponse
        where TDeleteRequest : DeleteEntryRequest
        where TDeleteResponse : DeleteEntryResponse
    {
        protected readonly AppDbContext _dbContext;
        protected readonly IFileStorageService _fileStorageService;

        public BaseEntryService(AppDbContext dbContext, IFileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _fileStorageService = fileStorageService;
        }

        public abstract Task<TNewResponse> Create(Guid userId, TNewRequest newRequest);

        public abstract Task<TGetResponse> Get(Guid userId, string id);

        //public abstract Task<TDeleteResponse> Delete(Guid userId, TDeleteRequest deleteRequest);

        protected async Task<IGetResponse?> FindByIdAsync(Guid userId, string id)
        {
            return await _dbContext
                .Set<TEntity>()
                .Where(e => e.EntityId == id && e.UserId == userId)
                .Include(e => e.Attachments)
                .Select(e => new GetEntryResponse
                {
                    Id = e.EntityId,
                    Date = e.Date,
                    Title = e.Title ?? "",
                    Content = e.Content ?? "",
                    Attachments = e
                        .Attachments.Where(a => a.FilePath != null)
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
                .FirstOrDefaultAsync();
        }

        protected async Task AddAsync(TEntity entity)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        protected async Task AddAttachmentsAsync(
            TEntity entity,
            TNewRequest newRequest,
            string sourceId
        )
        {
            if (newRequest.Attachments != null && newRequest.Attachments.Count > 0)
            {
                foreach (var file in newRequest.Attachments)
                {
                    var path = await _fileStorageService.SaveAttachment(file.File, sourceId);
                    var attachment = new Attachment
                    {
                        InternalFileName = path.InternalFileName,
                        OriginalFileName = file.File.FileName,
                        FilePath = path.FilePath,
                        ThumbnailPath = path.ThumbnailPath,
                        ContentHash = file.ContentHash,
                        CreatedAt = DateTime.UtcNow,
                    };

                    entity.LinkAttachment(attachment);
                    await _dbContext.Attachments.AddAsync(attachment);
                }

                await _dbContext.SaveChangesAsync();
            }
        }

        protected async Task RemoveAsync(TEntity entity)
        {
            _dbContext.Set<TEntity>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
