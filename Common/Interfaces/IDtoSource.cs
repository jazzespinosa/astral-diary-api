namespace AstralDiaryApi.Common.Interfaces
{
    public interface IRequestDto<TAttachment>
    {
        public DateOnly Date { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int Mood { get; set; }
        public List<TAttachment>? Attachments { get; set; }
    }

    public abstract class BaseRequestDto<TAttachment> : IRequestDto<TAttachment>
    {
        public DateOnly Date { get; set; }
        public virtual string? Title { get; set; }
        public virtual string? Content { get; set; }
        public int Mood { get; set; }
        public List<TAttachment>? Attachments { get; set; }
    }

    public class AttachmentObjRequest
    {
        public required string ContentHash { get; set; }
        public required IFormFile File { get; set; }
    }

    public interface IResponseDto
    {
        public string Id { get; set; }
        public DateOnly Date { get; set; }
        public string? Title { get; set; }
    }

    public abstract class BaseResponseDto : IResponseDto
    {
        public required string Id { get; set; }
        public DateOnly Date { get; set; }
        public virtual string? Title { get; set; }
    }

    public interface IGetRequest
    {
        public string Id { get; set; }
    }

    public interface IGetResponse
    {
        public string Id { get; set; }
        public DateOnly Date { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int Mood { get; set; }
        public ICollection<AttachmentObjResponse>? Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public class AttachmentObjResponse
    {
        public required string FilePath { get; set; }
        public required string ThumbnailPath { get; set; }
        public required string InternalFileName { get; set; }
        public required string OriginalFileName { get; set; }
    }

    public abstract class BaseGetResponse : IGetResponse
    {
        public required string Id { get; set; }
        public DateOnly Date { get; set; }
        public virtual string? Title { get; set; }
        public virtual string? Content { get; set; }
        public int Mood { get; set; }
        public ICollection<AttachmentObjResponse>? Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public interface IUpdateRequest<TAttachment> : IRequestDto<TAttachment>
    {
        public string Id { get; set; }
    }

    public interface IUpdateResponse : IResponseDto { }
}
