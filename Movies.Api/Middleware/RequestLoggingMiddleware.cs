namespace Movies.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var start = DateTime.UtcNow;

            var requestId = Guid.NewGuid().ToString("N");
            context.TraceIdentifier = requestId;

            _logger.LogInformation(
                "Request started: {Method} {Path} RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                requestId);

            try
            {
                await _next(context);

                var elapsed = DateTime.UtcNow - start;

                _logger.LogInformation(
                    "Request completed: {Method} {Path} StatusCode: {StatusCode} DurationMs: {DurationMs} RequestId: {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsed.TotalMilliseconds,
                    requestId);
            }
            catch (Exception ex)
            {
                var elapsed = DateTime.UtcNow - start;

                _logger.LogError(
                    ex,
                    "Request failed: {Method} {Path} DurationMs: {DurationMs} RequestId: {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsed.TotalMilliseconds,
                    requestId);

                throw;
            }
        }
    }
}