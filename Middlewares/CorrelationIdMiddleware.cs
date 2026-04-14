using System.Diagnostics;
using Microsoft.Extensions.Primitives;

namespace ServiceApi.API.Middleware;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware>? _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware>? logger = null)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prefer inbound header if present (validate format to avoid header abuse)
        static bool IsSafe(string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return false;
            if (v.Length > 64) return false;
            foreach (var ch in v)
            {
                if (!(char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')) return false;
            }
            return true;
        }

        string correlationId;
        if (context.Request.Headers.TryGetValue(HeaderName, out StringValues existing) &&
            !StringValues.IsNullOrEmpty(existing) &&
            IsSafe(existing.ToString()))
        {
            correlationId = existing.ToString();
        }
        else
        {
            correlationId = Guid.NewGuid().ToString("N");
            context.Request.Headers[HeaderName] = correlationId;
        }

        // Surface to response and tracing
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Set TraceIdentifier so downstream logs can include it
        context.TraceIdentifier = correlationId;

        // Optional Activity.Current Id set for distributed tracing correlation
        if (Activity.Current is { } activity)
        {
            activity.SetTag("correlation.id", correlationId);
        }

        // Begin a logging scope so structured loggers can include the correlation id
        using (_logger?.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }
}
