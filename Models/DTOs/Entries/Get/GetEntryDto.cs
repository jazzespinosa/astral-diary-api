using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.DTOs.Entries.Get
{
    public class GetEntryRequest
    {
        public required string EntryId { get; set; }
    }

    public class GetEntryResponse : GetResponse
    {
        public string EntryId
        {
            get => Id;
            set => Id = value;
        }
    }

    public class GetEntryIdResponse
    {
        public int Id { get; set; }
        public required string EntryId { get; set; }
    }
}
