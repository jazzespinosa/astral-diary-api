using AstralDiaryApi.Models.DTOs.Entries.Get;
using AstralDiaryApi.Models.DTOs.Entries.New;
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
        public async Task<IActionResult> CreateEntry([FromForm] NewEntryRequest newEntryRequest)
        {
            var userId = await GetUserId();
            var response = await _entryService.Create(userId, newEntryRequest);

            return Ok(response);
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

        [HttpGet("get-entry/{entryId}")]
        [Authorize]
        public async Task<IActionResult> GetEntry(string entryId)
        {
            var userId = await GetUserId();
            var response = await _entryService.Get(userId, entryId);

            return Ok(response);
        }

        private async Task<Guid> GetUserId()
        {
            var firebaseUid = User.FindFirst("user_id")?.Value ?? string.Empty;
            return await _userService.GetUserId(firebaseUid);
        }
    }
}
