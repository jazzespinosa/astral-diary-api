using System.IO.Hashing;

namespace AstralDiaryApi.Common.Helpers
{
    public static class HashHelper
    {
        public static async Task<string> GenerateContentHashAsync(this IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("File is empty or null");

            var content = new byte[file.Length];
            using var stream = file.OpenReadStream();
            await stream.ReadExactlyAsync(content);

            var hashBytes = XxHash64.Hash(content);

            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}
