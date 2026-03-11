namespace AstralDiaryApi.Models.DTOs.Attachments
{
    public class GetAttachmentDto { }

    public class FileDownloadResult
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
    }
}
