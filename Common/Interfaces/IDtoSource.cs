namespace AstralDiaryApi.Common.Interfaces
{
    public interface INewRequest
    {
        public DateOnly Date { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<AttachmentObjRequest>? Attachments { get; set; }
    }

    public interface INewResponse
    {
        public string Id { get; set; }
        public DateOnly Date { get; set; }
        public string? Title { get; set; }
    }

    public class AttachmentObjRequest
    {
        public required string ContentHash { get; set; }
        public required IFormFile File { get; set; }
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
        public ICollection<AttachmentObjResponse>? Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public class AttachmentObjResponse
    {
        public required string FilePath { get; set; }
        public required string ThumbnailPath { get; set; }
        public required string InternalFileName { get; set; }
    }
}
