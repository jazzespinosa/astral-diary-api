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
        int Mood { get; set; }
        string EncryptedContent { get; set; }
        string ContentIv { get; set; }
        string ContentSalt { get; set; }
        string? AttachmentId { get; set; }
        string? ThumbnailId { get; set; }
        string? AttachmentHash { get; set; }
    }
}
