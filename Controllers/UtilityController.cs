using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Models.DTOs.Utility;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UtilityController : BaseAppController
    {
        private readonly IUtilityService _utilityService;

        public UtilityController(IUserService userService, IUtilityService utilityService)
            : base(userService)
        {
            _utilityService = utilityService;
        }

        [HttpPost("feedback")]
        [Authorize]
        public async Task<IActionResult> SendEmail([FromBody] FeedbackRequest feedbackRequestDto)
        {
            var userId = await GetUserId();
            var response = await _utilityService.TriggerEmailSend(feedbackRequestDto, userId);
            return Ok(response);
        }

        [HttpGet("online")]
        public async Task<IActionResult> OnlineCheck()
        {
            return Ok(new { status = "online" });
        }
    }
}
