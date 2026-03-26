namespace AstralDiaryApi.Models.DTOs.Attachments
{
    public class AttachmentComparisonDto
    {
        public required string OriginalFileName { get; set; }
        public required string InternalFileName { get; set; }
        public required string ContentHash { get; set; }
    }
}
