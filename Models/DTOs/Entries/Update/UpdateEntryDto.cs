using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.DTOs.Entries.Update
{
    public class UpdateEntryRequest : BaseRequestDto
    {
        public required string Id { get; set; }
    }

    public class UpdateEntryResponse : BaseResponseDto
    {
        public string EntryId
        {
            get => Id;
            set => Id = value;
        }
    }

    public class UpdateDraftRequest : BaseRequestDto
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
