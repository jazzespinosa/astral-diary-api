using System.Net;
using AstralDiaryApi.Common.Helpers;
using AstralDiaryApi.Common.Interfaces;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Google.Apis.Requests.BatchRequest;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntryController : ControllerBase
    {
        private readonly IEntryService _entryService;
        private readonly IUserService _userService;

        public EntryController(IEntryService entryService, IUserService userService)
        {
            _entryService = entryService;
            _userService = userService;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateEntry(
            [FromForm] NewEntryRequestRaw newEntryRequestRaw
        )
        {
            var userId = await GetUserId();
            var processedRequestDto = new NewEntryRequestProcessed
            {
                Date = newEntryRequestRaw.Date,
                Title = newEntryRequestRaw.Title,
                Content = newEntryRequestRaw.Content,
                Mood = newEntryRequestRaw.Mood,
                Attachments = await AttachmentHelper.ProcessAttachmentsAsync(
                    newEntryRequestRaw.Attachments
                ),
            };

            var response = await _entryService.Create(userId, processedRequestDto);
            var uri = Url.Action("GetEntry", "Entry", new { entryId = response.Id });

            return Created(uri, response);
        }

        [HttpGet("get-calendar-entries")]
        [Authorize]
        public async Task<IActionResult> GetCalendarEntries(DateOnly date)
        {
            var userId = await GetUserId();
            var response = await _entryService.GetCalendarEntries(userId, date);
            return Ok(response);
        }

        [HttpGet("get-recent-entries")]
        [Authorize]
        public async Task<IActionResult> GetRecentEntries()
        {
            var userId = await GetUserId();
            var response = await _entryService.GetRecentEntries(userId, 10);

            return Ok(response);
        }

        [HttpGet("get-search-entries")]
        [Authorize]
        public async Task<IActionResult> GetSearchEntries(
            [FromQuery] GetSearchEntryRequest getSearchEntriesRequest
        )
        {
            var userId = await GetUserId();
            if (getSearchEntriesRequest.PageSize > 100)
                getSearchEntriesRequest.PageSize = 100;
            var response = await _entryService.SearchAsync(userId, getSearchEntriesRequest);

            return Ok(response);
        }

        [HttpGet("get/{entryId}")]
        [Authorize]
        public async Task<IActionResult> GetEntry(string entryId)
        {
            var userId = await GetUserId();
            var response = await _entryService.Get(userId, entryId);

            return Ok(response);
        }

        [HttpPut("update/{entryId}")]
        [Authorize]
        public async Task<IActionResult> UpdateEntry(
            string entryId,
            [FromForm] UpdateEntryRequestRaw updateEntryRequestRaw
        )
        {
            if (entryId != updateEntryRequestRaw.Id)
                return BadRequest("Entry Id does not match.");

            var userId = await GetUserId();
            var processedRequestDto = new UpdateEntryRequestProcessed
            {
                Id = updateEntryRequestRaw.Id,
                Date = updateEntryRequestRaw.Date,
                Title = updateEntryRequestRaw.Title,
                Content = updateEntryRequestRaw.Content,
                Mood = updateEntryRequestRaw.Mood,
                Attachments = await AttachmentHelper.ProcessAttachmentsAsync(
                    updateEntryRequestRaw.Attachments
                ),
            };

            var response = await _entryService.Update(userId, processedRequestDto);

            return Ok(response);
        }

        [HttpDelete("delete/{entryId}")]
        [Authorize]
        public async Task<IActionResult> DeleteEntry(string entryId)
        {
            var userId = await GetUserId();
            var response = await _entryService.SoftDeleteEntry(userId, entryId);

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
