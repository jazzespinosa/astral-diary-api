using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.DTOs.Entries.New
{
    public class NewEntryRequestRaw : BaseRequestDto<IFormFile>
    {
        public override required string Title { get; set; }
        public override required string Content { get; set; }
    }

    public class NewEntryRequestProcessed : BaseRequestDto<AttachmentObjRequest>
    {
        public override required string Title { get; set; }
        public override required string Content { get; set; }
    }

    public class NewEntryResponse : BaseResponseDto
    {
        public string EntryId
        {
            get => Id;
            set => Id = value;
        }
        public override required string Title { get; set; }
    }

    public class NewDraftRequestRaw : BaseRequestDto<IFormFile> { }

    public class NewDraftRequestProcessed : BaseRequestDto<AttachmentObjRequest> { }

    public class NewDraftResponse : BaseResponseDto
    {
        public string DraftId
        {
            get => Id;
            set => Id = value;
        }
    }
}
