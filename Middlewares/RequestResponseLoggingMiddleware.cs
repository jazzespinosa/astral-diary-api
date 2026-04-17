using System.Text;

namespace AstralDiaryApi.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
        }

        // InvokeAsync runs on EVERY incoming request
        public async Task InvokeAsync(HttpContext context)
        {
            await LogRequest(context);

            await _next(context);

            await LogResponse(context);
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            var body = string.Empty;

            var requestContentType = context.Request.ContentType ?? string.Empty;

            var isFormData = requestContentType.Contains(
                "multipart/form-data",
                StringComparison.OrdinalIgnoreCase
            );
            if (isFormData)
            {
                body = "[***ENCRYPTED BLOB***]";
            }
            else if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    leaveOpen: true
                );

                body = await reader.ReadToEndAsync();

                context.Request.Body.Position = 0;
            }

            _logger.LogInformation(
                "──────────────────────────────────────────\n"
                    + "INCOMING REQUEST\n"
                    + "   Method  : {Method}\n"
                    + "   Path    : {Path}\n"
                    + "   Query   : {Query}\n"
                    + "   Headers : {Headers}\n"
                    + "   Body    : {Body}\n"
                    + "──────────────────────────────────────────",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                FormatHeaders(context.Request.Headers),
                string.IsNullOrWhiteSpace(body) ? "(empty)" : body
            );
        }

        private async Task LogResponse(HttpContext context)
        {
            _logger.LogInformation(
                "──────────────────────────────────────────\n"
                    + "OUTGOING RESPONSE\n"
                    + "   Status  : {StatusCode}\n"
                    + "   Headers : {Headers}\n"
                    + "──────────────────────────────────────────",
                context.Response.StatusCode,
                FormatHeaders(context.Response.Headers)
            );
        }

        private static string FormatHeaders(IHeaderDictionary headers)
        {
            var sensitive = new[] { "Authorization" };
            return string.Join(
                ", ",
                headers.Select(h =>
                    sensitive.Contains(h.Key) ? $"{h.Key}=[***REDACTED***]" : $"{h.Key}={h.Value}"
                )
            );
        }
    }
}
