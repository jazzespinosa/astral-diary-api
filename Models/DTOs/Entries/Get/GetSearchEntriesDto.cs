namespace AstralDiaryApi.Models.DTOs.Entries.Get
{
    public class GetSearchEntryRequest
    {
        public string? SearchTerm { get; set; }
        public string DateFilter { get; set; } = "any";
        public DateOnly? Date { get; set; }
        public int Mood { get; set; }
        public string Sort { get; set; } = "desc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
