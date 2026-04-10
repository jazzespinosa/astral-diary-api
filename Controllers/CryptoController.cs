using System.Security.Cryptography;
using System.Text;
using AstralDiaryApi.Services.Implementations;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CryptoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public CryptoController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        private string ServerSecret => _configuration["Crypto:ServerPepperSecret"]!;

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

        private async Task<Guid> GetUserId()
        {
            var firebaseUid = User.FindFirst("user_id")?.Value ?? string.Empty;
            return await _userService.GetUserId(firebaseUid);
        }
    }
}
