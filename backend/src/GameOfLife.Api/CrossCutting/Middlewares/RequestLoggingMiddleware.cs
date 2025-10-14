using System.Diagnostics;
using System.Text;

namespace GameOfLife.CrossCutting.Middlewares;

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
        var stopwatch = Stopwatch.StartNew();

        var request = context.Request;
        _logger.LogInformation("➡️ {Method} {Path}", request.Method, request.Path);

        if (request.ContentLength > 0 && request.ContentType?.Contains("application/json") == true)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            _logger.LogDebug("Request body: {Body}", body);
            request.Body.Position = 0;
        }

        await _next(context);

        stopwatch.Stop();
        _logger.LogInformation("⬅️ {Method} {Path} responded {StatusCode} in {Elapsed}ms",
            request.Method,
            request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
