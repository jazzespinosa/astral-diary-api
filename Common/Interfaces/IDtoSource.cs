namespace AstralDiaryApi.Common.Interfaces
{
    public interface IRequestDto
    {
        public DateOnly Date { get; set; }
        public int Mood { get; set; }
        public string EncryptedContent { get; set; }
        public string ContentIv { get; set; }
        public string ContentSalt { get; set; }
        public IFormFile? EncryptedAttachments { get; set; }
        public IFormFile? EncryptedThumbnails { get; set; }
        public string? AttachmentHash { get; set; }
    }

    public abstract class BaseRequestDto : IRequestDto
    {
        public DateOnly Date { get; set; }
        public int Mood { get; set; }
        public required string EncryptedContent { get; set; }
        public required string ContentIv { get; set; }
        public required string ContentSalt { get; set; }
        public IFormFile? EncryptedAttachments { get; set; }
        public IFormFile? EncryptedThumbnails { get; set; }
        public string? AttachmentHash { get; set; }
    }

    public interface IResponseDto
    {
        public string Id { get; set; }
    }

    public abstract class BaseResponseDto : IResponseDto
    {
        public required string Id { get; set; }
    }

    public interface IGetRequest
    {
        public string Id { get; set; }
    }

    public interface IGetResponse
    {
        public string Id { get; set; }
        public DocuType DocuType { get; set; }
        public DateOnly Date { get; set; }
        public int Mood { get; set; }
        public string EncryptedContent { get; set; }
        public string ContentIv { get; set; }
        public string ContentSalt { get; set; }
        public string? AttachmentId { get; set; }
        public string? AttachmentHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class GetResponse : IGetResponse
    {
        public required string Id { get; set; }
        public DocuType DocuType { get; set; }

        public string Type => DocuType.ToString();

        public DateOnly Date { get; set; }
        public int Mood { get; set; }
        public required string EncryptedContent { get; set; }
        public required string ContentIv { get; set; }
        public required string ContentSalt { get; set; }
        public string? AttachmentId { get; set; }
        public string? AttachmentHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    public class AttachmentObject
    {
        public required string AttachmentPath { get; set; }
        public required string ThumbnailPath { get; set; }
        public required string AttachmentId { get; set; }
    }

    public interface IUpdateRequest : IRequestDto
    {
        public string Id { get; set; }
    }

    public interface IUpdateResponse : IResponseDto { }

    public enum DocuType
    {
        Entry,
        Draft,
    }
}
