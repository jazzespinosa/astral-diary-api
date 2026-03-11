using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Models.Enums;

namespace AstralDiaryApi.Models.DTOs.Entries.New
{
    public class NewEntryRequest : BaseNewRequest
    {
        public override required string Title { get; set; }
        public override required string Content { get; set; }
    }

    public class NewEntryResponse : BaseNewResponse
    {
        public string EntryId
        {
            get => Id;
            set => Id = value;
        }
        public override required string Title { get; set; }
    }

    public class NewDraftRequest : BaseNewRequest { }

    public class NewDraftResponse : BaseNewResponse
    {
        public string DraftId
        {
            get => Id;
            set => Id = value;
        }
    }
}
