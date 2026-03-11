namespace AstralDiaryApi.Common.Interfaces
{
    public interface IBaseEntryService<
        TEntity,
        TNewRequest,
        TNewResponse,
        TGetResponse,
        TUpdateRequest,
        TUpdateResponse,
        TDeleteRequest,
        TDeleteResponse
    >
    {
        Task<TNewResponse> Create(Guid userId, TNewRequest newRequest);
        Task<TGetResponse> Get(Guid userId, string id);

        //Task<TUpdateResponse> UpdateEntry(Guid userId, TUpdateRequest updateRequest);
        //Task<TDeleteResponse> DeleteEntry(Guid userId, TDeleteRequest deleteRequest);
    }
}
