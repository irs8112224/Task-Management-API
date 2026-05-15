namespace TaskManagement.Api.Middlewares;

public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Store for downstream usage
        context.Items[HeaderName] = correlationId;

        // Ensure response header is always returned
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Push into logs (Serilog + ILogger support)
        using (_logger.BeginScope("{CorrelationId}", correlationId))
        {
            await _next(context);
        }
    }
}