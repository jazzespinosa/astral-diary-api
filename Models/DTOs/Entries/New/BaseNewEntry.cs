using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.DTOs.Entries.New
{
    public abstract class BaseNewRequest : INewRequest
    {
        public DateOnly Date { get; set; }
        public virtual string? Title { get; set; }
        public virtual string? Content { get; set; }
        public List<AttachmentObjRequest>? Attachments { get; set; }
    }

    public abstract class BaseNewResponse : INewResponse
    {
        public required string Id { get; set; }
        public DateOnly Date { get; set; }
        public virtual string? Title { get; set; }
    }
}
