using System.Security.Cryptography;
using System.Text;
using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.env;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CryptoController : BaseAppController
    {
        private readonly MyEnvironment _env;

        public CryptoController(IUserService userService, MyEnvironment env)
            : base(userService)
        {
            _env = env;
        }

        private string ServerSecret => _env.ServerSecret;

        [HttpGet("pepper")]
        [Authorize]
        public async Task<IActionResult> GetPepper()
        {
            var uid = await GetUserId();
            var uidString = uid.ToString();

            if (uidString is null)
                return Unauthorized();

            var keyBytes = Encoding.UTF8.GetBytes(ServerSecret);
            var uidBytes = Encoding.UTF8.GetBytes(uidString);
            var pepperBytes = HMACSHA256.HashData(keyBytes, uidBytes);
            var pepper = Convert.ToBase64String(pepperBytes);

            return Ok(new { pepper });
        }
    }
}
