using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Models.DTOs.Users;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : BaseAppController
    {
        private readonly IEntryService _entryService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            IEntryService entryService,
            ILogger<UserController> logger
        )
            : base(userService)
        {
            _entryService = entryService;
            _logger = logger;
        }

        [HttpPost("login")]
        [Authorize]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            _logger.LogInformation("Executing Login...");

            if (!IsEmailVerified())
                return BadRequest("Email is not verified.");

            _logger.LogInformation("Email verified.");

            var firebaseUid = GetFirebaseUid();

            _logger.LogInformation("Firebase UID 0: {0}", firebaseUid);

            var response = await _userService.LoginUser(firebaseUid, loginRequest);
            return Ok(response);
        }

        [HttpGet("get-user-info")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo([FromQuery] DateOnly currentDate)
        {
            var userId = await GetUserId();

            var response = await _userService.GetUserInfoAsync(userId, currentDate);
            return Ok(response);
        }

        [HttpGet("get-mood-map")]
        [Authorize]
        public async Task<IActionResult> GetUserMoodMap([FromQuery] int year)
        {
            var userId = await GetUserId();

            var response = await _entryService.GetUserMoodMapAsync(userId, year);
            return Ok(response);
        }

        [HttpGet("get-avatar")]
        [Authorize]
        public async Task<IActionResult> GetUserAvatar()
        {
            var userId = await GetUserId();
            var response = await _userService.GetUserAvatar(userId);
            return Ok(new { avatar = response });
        }

        [HttpPatch("save-avatar")]
        [Authorize]
        public async Task<IActionResult> SaveUserAvatar(
            [FromBody] UpdateUserAvatarRequest updateUserAvatarRequest
        )
        {
            var userId = await GetUserId();
            var response = await _userService.UpdateUserAvatar(
                userId,
                updateUserAvatarRequest.Avatar
            );

            return Ok(new { avatar = response });
        }

        private bool IsEmailVerified() => User.FindFirst("email_verified")?.Value == "true";

        private string GetFirebaseUid() => User.FindFirst("user_id")?.Value ?? string.Empty;
    }
}
