namespace AstralDiaryApi.Common.Interfaces
{
    public interface IBaseEntryService<
        TEntity,
        TNewRequest,
        TNewResponse,
        TGetResponse,
        TUpdateRequest,
        TUpdateResponse
    >
    {
        Task<TNewResponse> Create(Guid userId, TNewRequest newRequest);
        Task<TGetResponse> Get(Guid userId, string entityId);
        Task<TUpdateResponse> Update(Guid userId, TUpdateRequest updateRequest);
    }
}
