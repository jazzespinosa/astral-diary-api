using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.DTOs.Entries.Get
{
    public abstract class BaseGetRequest { }

    public abstract class BaseGetResponse : IGetResponse
    {
        public required string Id { get; set; }
        public DateOnly Date { get; set; }
        public virtual string? Title { get; set; }
        public virtual string? Content { get; set; }
        public ICollection<AttachmentObjResponse>? Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
