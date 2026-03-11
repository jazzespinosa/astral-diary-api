using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Services.Implementations;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DraftController : ControllerBase
    {
        private readonly IDraftService _draftService;
        private readonly IUserService _userService;

        public DraftController(IDraftService draftService, IUserService userService)
        {
            _draftService = draftService;
            _userService = userService;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateDraft([FromForm] NewDraftRequest newDraftRequest)
        {
            var userId = await GetUserId();
            var response = await _draftService.Create(userId, newDraftRequest);

            return Ok(response);
        }

        [HttpGet("get-all")]
        [Authorize]
        public async Task<IActionResult> GetAllDrafts()
        {
            var userId = await GetUserId();
            var response = await _draftService.GetDrafts(userId);
            return Ok(response);
        }

        [HttpGet("get-draft/{draftId}")]
        [Authorize]
        public async Task<IActionResult> GetDraft(string draftId)
        {
            var userId = await GetUserId();
            var response = await _draftService.Get(userId, draftId);

            return Ok(response);
        }

        private async Task<Guid> GetUserId()
        {
            var firebaseUid = User.FindFirst("user_id")?.Value ?? string.Empty;
            return await _userService.GetUserId(firebaseUid);
        }
    }
}
