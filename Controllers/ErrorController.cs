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
                return StatusCode(500, "Unknown error occurred.");

            _logger.LogError(exception, "Unhandled exception");

            return exception switch
            {
                DbUpdateConcurrencyException => Conflict(
                    "The record was modified by another user."
                ),
                DbUpdateException => StatusCode(500, "A database error occurred."),
                InvalidOperationException ex => StatusCode(500, new { message = ex.Message }),
                ArgumentException argEx => BadRequest(new { message = argEx.Message }),
                MaxItemsExceededException ex => StatusCode(409, new { message = ex.Message }),
                UnauthorizedAccessException ex => StatusCode(401, new { message = ex.Message }),
                NotFoundException ex => StatusCode(404, new { message = ex.Message }),
                _ => StatusCode(500, "Something went wrong."),
            };
        }
    }
}
