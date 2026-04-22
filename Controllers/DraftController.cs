using AstralDiaryApi.Common.Generics;
using AstralDiaryApi.Common.Helpers;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DraftController : BaseAppController
    {
        private readonly IDraftService _draftService;

        public DraftController(IDraftService draftService, IUserService userService)
            : base(userService)
        {
            _draftService = draftService;
        }

        [HttpPost("create")]
        [Authorize]
        [RequestSizeLimit(15 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 15 * 1024 * 1024)]
        public async Task<IActionResult> CreateDraft([FromForm] NewDraftRequest newDraftRequest)
        {
            var userId = await GetUserId();

            var response = await _draftService.Create(userId, newDraftRequest);

            return Ok(response);
        }

        [HttpGet("count")]
        [Authorize]
        public async Task<IActionResult> CountDrafts()
        {
            var userId = await GetUserId();
            var response = await _draftService.CountDraftsAsync(userId);

            return Ok(new { Count = response });
        }

        [HttpGet("get-all")]
        [Authorize]
        public async Task<IActionResult> GetAllDrafts()
        {
            var userId = await GetUserId();
            var response = await _draftService.GetAllDrafts(userId);
            return Ok(response);
        }

        [HttpGet("get/{draftId}")]
        [Authorize]
        public async Task<IActionResult> GetDraft(string draftId)
        {
            var userId = await GetUserId();
            var response = await _draftService.Get(userId, draftId);

            return Ok(response);
        }

        [HttpPut("update/{draftId}")]
        [Authorize]
        [RequestSizeLimit(15 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 15 * 1024 * 1024)]
        public async Task<IActionResult> UpdateDraft(
            string draftId,
            [FromForm] UpdateDraftRequest updateDraftRequest
        )
        {
            if (draftId != updateDraftRequest.Id)
                return BadRequest("Draft Id does not match.");

            var userId = await GetUserId();

            var response = await _draftService.Update(userId, updateDraftRequest);

            return Ok(response);
        }

        [HttpPost("publish/{draftId}")]
        [Authorize]
        [RequestSizeLimit(15 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 15 * 1024 * 1024)]
        public async Task<IActionResult> PublishDraft(
            string draftId,
            [FromForm] UpdateDraftRequest updateDraftRequest
        )
        {
            if (draftId != updateDraftRequest.Id)
                return BadRequest("Draft Id does not match.");

            var userId = await GetUserId();
            var dateNow = Request.GetUserLocalDate();

            if (updateDraftRequest.Date > dateNow)
            {
                return BadRequest("Entry date cannot be in the future.");
            }

            var response = await _draftService.PublishDraft(userId, updateDraftRequest);

            return Ok(response);
        }

        [HttpDelete("delete/{draftId}")]
        [Authorize]
        public async Task<IActionResult> DeleteDraft(string draftId)
        {
            var userId = await GetUserId();
            var response = await _draftService.DeleteDraft(userId, draftId);

            if (response)
                return StatusCode(StatusCodes.Status204NoContent);

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
