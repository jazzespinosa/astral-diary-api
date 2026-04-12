using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.DTOs.Entries.Update
{
    public class UpdateEntryRequest : BaseRequestDto, IUpdateRequest
    {
        public required string Id { get; set; }
    }

    public class UpdateEntryResponse : BaseResponseDto, IUpdateResponse
    {
        public string EntryId
        {
            get => Id;
            set => Id = value;
        }
    }

    public class UpdateDraftRequest : BaseRequestDto, IUpdateRequest
    {
        public required string Id { get; set; }
    }

    public class UpdateDraftResponse : BaseResponseDto, IUpdateResponse
    {
        public string DraftId
        {
            get => Id;
            set => Id = value;
        }
    }
}
