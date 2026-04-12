using System.ComponentModel.DataAnnotations;

namespace AstralDiaryApi.Models.DTOs.Entries.Get
{
    public class GetSearchEntryRequest
    {
        public required DateTypeFilter DateFilter { get; set; } = DateTypeFilter.any;
        public DateOnly? Date { get; set; }

        [Range(0, 5, ErrorMessage = "Mood must be between 0 and 5")]
        public int? Mood { get; set; }
        public required SortType Sort { get; set; } = SortType.desc;
    }

    public class PagedResult
    {
        public List<GetEntryResponse> Items { get; set; } = new();
    }

    public enum DateTypeFilter
    {
        any,
        exact,
        before,
        after,
    }

    public enum SortType
    {
        asc,
        desc,
    }
}
