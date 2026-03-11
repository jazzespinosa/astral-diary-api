using AstralDiaryApi.Models.Entities;

namespace AstralDiaryApi.Common.Interfaces
{
    public interface IAttachmentSource
    {
        int Id { get; set; }
        ICollection<Attachment> Attachments { get; set; }
        public void LinkAttachment(Attachment attachment);
    }
}
