using AstralDiaryApi.Common.Interfaces;

namespace AstralDiaryApi.Models.DTOs.Entries.Get
{
    public class GetDraftRequest { }

    public class GetDraftResponse : GetResponse
    {
        public string DraftId
        {
            get => Id;
            set => Id = value;
        }
    }

    public class GetDraftCountResponse
    {
        public int Count { get; set; }
    }
}
