namespace AstralDiaryApi.Common.Interfaces
{
    public interface IBaseEntryService<TEntity>
    {
        Task<IResponseDto> Create(Guid userId, IRequestDto newRequest);
        Task<IGetResponse> Get(Guid userId, string entityId);
        Task<IUpdateResponse> Update(Guid userId, IUpdateRequest updateRequest);
    }
}
