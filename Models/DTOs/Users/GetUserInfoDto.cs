namespace AstralDiaryApi.Models.DTOs.Users
{
    public class GetUserInfoResponse
    {
        public required string Email { get; set; }
        public required string DisplayName { get; set; }
        public string? Avatar { get; set; }
        public int TotalEntries { get; set; }
        public string? FirstEntryId { get; set; }
        public DateOnly? FirstEntryDate { get; set; }
        public string? LatestEntryId { get; set; }
        public DateOnly? LatestEntryDate { get; set; }
        public int CurrentStreak { get; set; }
    }

    public class UserInitialDetailsDto
    {
        public required string Email { get; set; }
        public required string DisplayName { get; set; }
        public string? Avatar { get; set; }
    }

    public class UserMoodMap
    {
        public DateOnly Date { get; set; }
        public int Mood { get; set; }
    }
}
