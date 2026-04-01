using System.Text;

namespace SupplyChain.WebApi.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var start = DateTime.UtcNow;

            // ===== READ REQUEST =====
            context.Request.EnableBuffering();

            string requestBody = "";
            if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    leaveOpen: true);

                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            var originalBody = context.Response.Body;

            using var newBody = new MemoryStream();
            context.Response.Body = newBody;

            try
            {
                await _next(context);
            }
            finally
            {
                var duration = (DateTime.UtcNow - start).TotalMilliseconds;

                newBody.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(newBody).ReadToEndAsync();

                newBody.Seek(0, SeekOrigin.Begin);
                await newBody.CopyToAsync(originalBody);

                _logger.LogInformation(
                    """
                HTTP {Method} {Path}
                Status: {StatusCode}
                Duration: {Duration} ms
                RequestBody: {RequestBody}
                ResponseBody: {ResponseBody}
                """,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    duration,
                    requestBody,
                    responseBody
                );
            }
        }
    }
}
