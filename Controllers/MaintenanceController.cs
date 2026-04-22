using AstralDiaryApi.Data;
using AstralDiaryApi.env;
using AstralDiaryApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<MaintenanceController> _logger;
        private readonly MyEnvironment _env;

        public MaintenanceController(
            AppDbContext appDbContext,
            IFileStorageService fileStorageService,
            ILogger<MaintenanceController> logger,
            MyEnvironment env
        )
        {
            _appDbContext = appDbContext;
            _fileStorageService = fileStorageService;
            _logger = logger;
            _env = env;
        }

        private string CronJobKey => _env.CronJobKey;

        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupDatabase(
            [FromHeader(Name = "X-API-Key")] string apiKey
        )
        {
            if (String.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Cleanup rejected: Missing CronJob-API-Key header");
                return Unauthorized(new { error = "Missing API key" });
            }

            if (!apiKey.Equals(CronJobKey))
            {
                _logger.LogWarning("Cleanup rejected: Invalid CronJob-API-Key");
                return Unauthorized(new { error = "Invalid API key" });
            }

            _logger.LogInformation("Cleanup authorized");

            try
            {
                // to update to [-30]; only for testing
                var daysToDelete = DateTime.UtcNow.AddDays(0);

                var entriesToDelete = await _appDbContext
                    .Entries.IgnoreQueryFilters()
                    .Where(r => r.DeletedAt < daysToDelete)
                    .ToListAsync();

                foreach (var entry in entriesToDelete)
                {
                    _appDbContext.Remove(entry);
                    await _fileStorageService.DeleteSavedAttachment(
                        entry.UserId,
                        entry.EntityId,
                        entry.AttachmentId,
                        entry.ThumbnailId
                    );
                }

                await _appDbContext.SaveChangesAsync();

                _logger.LogInformation($"Deleted {entriesToDelete.Count()} old records");

                return Ok(new { success = true, deleted = entriesToDelete.Count() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cleanup failed");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
