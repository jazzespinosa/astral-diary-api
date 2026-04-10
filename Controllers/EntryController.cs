using AstralDiaryApi.Common.Helpers;
using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
using AstralDiaryApi.Models.DTOs.Entries.Update;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> CreateEntry([FromForm] NewEntryRequest newEntryRequest)
        {
            var userId = await GetUserId();
            var dateNow = Request.GetUserLocalDate();

            if (newEntryRequest.Date > dateNow)
            {
                return BadRequest("Entry date cannot be in the future.");
            }

            var response = await _entryService.Create(userId, newEntryRequest);
            var uri = Url.Action("GetEntry", "Entry", new { entryId = response.Id });

            return Created(uri, response);
        }

        [HttpGet("get-calendar-entries")]
        [Authorize]
        public async Task<IActionResult> GetCalendarEntries([FromQuery] DateOnly date)
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

        [HttpGet("get/entry-ids")]
        [Authorize]
        public async Task<IActionResult> GetEntryIds()
        {
            var userId = await GetUserId();
            var response = await _entryService.GetEntryIds(userId);

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
            [FromForm] UpdateEntryRequest updateEntryRequest
        )
        {
            if (entryId != updateEntryRequest.Id)
                return BadRequest("Entry Id does not match.");

            var userId = await GetUserId();
            var dateNow = Request.GetUserLocalDate();

            if (updateEntryRequest.Date > dateNow)
            {
                return BadRequest("Entry date cannot be in the future.");
            }

            var response = await _entryService.Update(userId, updateEntryRequest);

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

        [HttpGet("get-deleted-entries")]
        [Authorize]
        public async Task<IActionResult> GetDeletedEntries()
        {
            var userId = await GetUserId();
            var response = await _entryService.GetDeletedEntries(userId);
            return Ok(response);
        }

        [HttpPut("restore")]
        [Authorize]
        public async Task<IActionResult> RestoreEntries([FromBody] string[] entryIds)
        {
            var userId = await GetUserId();
            var response = await _entryService.RestoreEntries(userId, entryIds);

            return Ok(new { result = response, message = "Entries restored successfully." });
        }

        private async Task<Guid> GetUserId()
        {
            var firebaseUid = User.FindFirst("user_id")?.Value ?? string.Empty;
            return await _userService.GetUserId(firebaseUid);
        }
    }
}
