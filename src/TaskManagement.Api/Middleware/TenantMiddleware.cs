using Serilog.Context;

namespace TaskManagement.Api.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("X-Tenant-Id header is required.");
            return;
        }

        // ✅ Store for controllers/services
        context.Items["X-Tenant-Id"] = tenantId;

        // 👇 This is the correct place
        using (LogContext.PushProperty("TenantId", tenantId))
        {
            await _next(context);
        }
    }
}