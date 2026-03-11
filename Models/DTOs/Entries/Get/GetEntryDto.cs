using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.DTOs.Entries.Get
{
    public class GetEntryRequest
    {
        public required string EntryId { get; set; }
    }

    public class GetEntryResponse : BaseGetResponse
    {
        public string EntryId
        {
            get => Id;
            set => Id = value;
        }
        public override required string Title { get; set; }
        public override required string Content { get; set; }
    }
}
