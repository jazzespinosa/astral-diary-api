using AstralDiaryApi.Common.Helpers;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
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
        public async Task<IActionResult> CreateDraft(
            [FromForm] NewDraftRequestRaw newDraftRequestRaw
        )
        {
            var userId = await GetUserId();
            var processedRequestDto = new NewDraftRequestProcessed
            {
                Date = newDraftRequestRaw.Date,
                Title = newDraftRequestRaw.Title,
                Content = newDraftRequestRaw.Content,
                Mood = newDraftRequestRaw.Mood,
                Attachments = await AttachmentHelper.ProcessAttachmentsAsync(
                    newDraftRequestRaw.Attachments
                ),
            };

            var response = await _draftService.Create(userId, processedRequestDto);

            return Ok(response);
        }

        [HttpGet("count")]
        [Authorize]
        public async Task<IActionResult> CountDrafts()
        {
            var userId = await GetUserId();
            var response = await _draftService.CountDraftsAsync(userId);

            return Ok(response);
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
        public async Task<IActionResult> UpdateDraft(
            string draftId,
            [FromForm] UpdateDraftRequestRaw updateDraftRequestRaw
        )
        {
            if (draftId != updateDraftRequestRaw.Id)
                return BadRequest("Draft Id does not match.");

            var userId = await GetUserId();
            var processedRequestDto = new UpdateDraftRequestProcessed
            {
                Id = updateDraftRequestRaw.Id,
                Date = updateDraftRequestRaw.Date,
                Title = updateDraftRequestRaw.Title,
                Content = updateDraftRequestRaw.Content,
                Mood = updateDraftRequestRaw.Mood,
                Attachments = await AttachmentHelper.ProcessAttachmentsAsync(
                    updateDraftRequestRaw.Attachments
                ),
            };

            var response = await _draftService.Update(userId, processedRequestDto);

            return Ok(response);
        }

        [HttpPost("publish/{draftId}")]
        [Authorize]
        public async Task<IActionResult> PublishDraft(
            string draftId,
            [FromForm] UpdateDraftRequestRaw updateDraftRequestRaw
        )
        {
            if (draftId != updateDraftRequestRaw.Id)
                return BadRequest("Draft Id does not match.");

            var userId = await GetUserId();
            var processedRequestDto = new UpdateDraftRequestProcessed
            {
                Id = updateDraftRequestRaw.Id,
                Date = updateDraftRequestRaw.Date,
                Title = updateDraftRequestRaw.Title,
                Content = updateDraftRequestRaw.Content,
                Mood = updateDraftRequestRaw.Mood,
                Attachments = await AttachmentHelper.ProcessAttachmentsAsync(
                    updateDraftRequestRaw.Attachments
                ),
            };

            var response = await _draftService.PublishDraft(userId, processedRequestDto);

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

        private async Task<Guid> GetUserId()
        {
            var firebaseUid = User.FindFirst("user_id")?.Value ?? string.Empty;
            return await _userService.GetUserId(firebaseUid);
        }
    }
}
