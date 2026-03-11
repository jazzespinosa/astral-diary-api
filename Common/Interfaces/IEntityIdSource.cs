namespace AstralDiaryApi.Common.Interfaces
{
    public interface IEntityIdSource
    {
        int Id { get; set; }
        string EntityId { get; set; }
        Guid UserId { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime ModifiedAt { get; set; }
        DateOnly Date { get; set; }
        string? Title { get; set; }
        string? Content { get; set; }
    }
}
