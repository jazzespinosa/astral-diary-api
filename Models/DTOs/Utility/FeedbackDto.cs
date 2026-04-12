namespace AstralDiaryApi.Models.DTOs.Utility
{
    public class FeedbackRequest
    {
        public required string Category { get; set; }
        public required string Message { get; set; }
    }

    public class FeedbackResponse
    {
        public long FeedbackId { get; set; } = GenerateFeedbackId();
        private static int _sequence = 0;
        private static readonly object _lock = new object();

        public static long GenerateFeedbackId()
        {
            lock (_lock)
            {
                _sequence = (_sequence + 1) % 1000; // keep within 3 digits
                return long.Parse(
                    DateTime.UtcNow.ToString("yyyyMMddHHmmss") + _sequence.ToString("D3")
                );
            }
        }
    }
}
