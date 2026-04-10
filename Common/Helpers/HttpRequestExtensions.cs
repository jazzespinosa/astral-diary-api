namespace AstralDiaryApi.Common.Helpers
{
    public static class HttpRequestExtensions
    {
        public static DateOnly GetUserLocalDate(this HttpRequest request)
        {
            int offset = 0;
            if (request.Headers.TryGetValue("X-Timezone-Offset", out var val))
            {
                int.TryParse(val, out offset);
            }
            return DateOnly.FromDateTime(DateTime.UtcNow.AddMinutes(-offset));
        }
    }
}
