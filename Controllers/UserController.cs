using AstralDiaryApi.Data;
using AstralDiaryApi.Models.DTOs.Users;
using AstralDiaryApi.Services.Implementations;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        [Authorize]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!IsEmailVerified())
                return BadRequest("Email is not verified.");

            string firebaseUid = GetFirebaseUid();
            var response = await _userService.LoginUser(firebaseUid, loginRequest);
            return Ok(response);
        }

        private bool IsEmailVerified() => User.FindFirst("email_verified")?.Value == "true";

        private string GetFirebaseUid() => User.FindFirst("user_id")?.Value ?? string.Empty;
    }
}
