using AstralDiaryApi.Models.DTOs.Attachments;

namespace AstralDiaryApi.Models.DTOs.Entries.Delete
{
    public class DeleteAllAttachmentsResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
    }
}
