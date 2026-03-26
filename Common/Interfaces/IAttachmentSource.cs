using AstralDiaryApi.Models.Entities;

namespace AstralDiaryApi.Common.Interfaces
{
    public interface IAttachmentSource
    {
        ICollection<Attachment> Attachments { get; set; }
        public void LinkAttachment(Attachment attachment);
    }
}
