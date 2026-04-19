namespace AstralDiaryApi.Models.DTOs.Attachments
{
    public class GetAttachmentDto { }

    public class FileDownloadResult
    {
        public Stream FileStream { get; set; }
        public string FileName { get; set; }
    }
}
