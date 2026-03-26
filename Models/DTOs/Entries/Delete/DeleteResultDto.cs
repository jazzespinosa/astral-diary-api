using AstralDiaryApi.Models.DTOs.Attachments;

namespace AstralDiaryApi.Models.DTOs.Entries.Delete
{
    public class DeleteAttachmentsResult
    {
        public bool SuccessAll { get; set; }
        public int DeletedCount { get; set; } = 0;
        public ICollection<AttachmentComparisonDto> FailedFiles { get; set; } =
            new List<AttachmentComparisonDto>();
    }

    public class DeleteAllAttachmentsResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
    }
}
