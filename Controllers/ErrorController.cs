using AstralDiaryApi.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Controllers
{
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/error")]
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpPatch, HttpHead, HttpOptions]
        public IActionResult HandleError()
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

            if (exception is null)
            {
                _logger.LogError("Unhandled exception");
                return StatusCode(500, "Unknown error occurred.");
            }

            _logger.LogError(exception, "Unhandled exception");

            switch (exception)
            {
                case DbUpdateConcurrencyException ex:
                    _logger.LogError(ex, "The record was modified by another user.");
                    return Conflict("The record was modified by another user.");
                case DbUpdateException ex:
                    _logger.LogError(ex, "A database error occurred.");
                    return StatusCode(500, "A database error occurred.");
                case InvalidOperationException ex:
                    _logger.LogError(ex, ex.Message);
                    return StatusCode(500, new { message = ex.Message });
                case ArgumentException ex:
                    _logger.LogWarning(ex, ex.Message);
                    return BadRequest(new { message = ex.Message });
                case MaxItemsExceededException ex:
                    _logger.LogWarning(ex, ex.Message);
                    return StatusCode(409, new { message = ex.Message });
                case UnauthorizedAccessException ex:
                    _logger.LogWarning(ex, ex.Message);
                    return StatusCode(401, new { message = ex.Message });
                case NotFoundException ex:
                    _logger.LogWarning(ex, ex.Message);
                    return StatusCode(404, new { message = ex.Message });
                default:
                    _logger.LogError(exception, "Something went wrong.");
                    return StatusCode(500, "Something went wrong.");
            }
        }
    }
}
