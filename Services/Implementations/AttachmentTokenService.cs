using System.Net.Sockets;
using AstralDiaryApi.Models.DTOs.Attachments;
using AstralDiaryApi.Models.Entities;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AstralDiaryApi.Services.Implementations
{
    public class AttachmentTokenService : IAttachmentTokenService
    {
        private readonly IMemoryCache _cache;

        public AttachmentTokenService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public string CreateToken(Guid userId, string id)
        {
            var token = Guid.NewGuid().ToString("N");
            var data = new AttachmentTokenObject { UserId = userId, Id = id };

            _cache.Set(token, data, TimeSpan.FromSeconds(60));
            return token;
        }

        public AttachmentTokenObject ValidateToken(string token)
        {
            if (_cache.TryGetValue(token, out AttachmentTokenObject data))
            {
                //_cache.Remove(token);
                return data;
            }
            return null;
        }
    }
}
