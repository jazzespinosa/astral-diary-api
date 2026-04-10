using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.DTOs.Entries.New
{
    public class NewEntryRequest : BaseRequestDto { }

    public class NewEntryResponse : BaseResponseDto
    {
        public string EntryId
        {
            get => Id;
            set => Id = value;
        }
    }

    public class NewDraftRequest : BaseRequestDto { }

    public class NewDraftResponse : BaseResponseDto
    {
        public string DraftId
        {
            get => Id;
            set => Id = value;
        }
    }
}
