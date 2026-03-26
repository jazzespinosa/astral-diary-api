using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.DTOs.Entries.Update
{
    public class UpdateEntryRequestRaw : BaseRequestDto<IFormFile>
    {
        public required string Id { get; set; }
        public override required string Title { get; set; }
        public override required string Content { get; set; }
    }

    public class UpdateEntryRequestProcessed : BaseRequestDto<AttachmentObjRequest>
    {
        public required string Id { get; set; }
        public override required string Title { get; set; }
        public override required string Content { get; set; }
    }

    public class UpdateEntryResponse : BaseResponseDto
    {
        public string EntryId
        {
            get => Id;
            set => Id = value;
        }
        public override required string Title { get; set; }
    }

    public class UpdateDraftRequestRaw : BaseRequestDto<IFormFile>
    {
        public required string Id { get; set; }
    }

    public class UpdateDraftRequestProcessed : BaseRequestDto<AttachmentObjRequest>
    {
        public required string Id { get; set; }
    }

    public class UpdateDraftResponse : BaseResponseDto
    {
        public string DraftId
        {
            get => Id;
            set => Id = value;
        }
    }
}
