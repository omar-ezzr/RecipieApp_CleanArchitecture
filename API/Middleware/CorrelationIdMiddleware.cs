using System.Diagnostics;
using System.Text.RegularExpressions;

namespace API.Middleware;

public sealed partial class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";
    private const int MaximumLength = 128;
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetCorrelationId(context.Request.Headers[HeaderName]);
        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope("CorrelationId: {CorrelationId}", correlationId))
        {
            await _next(context);
        }
    }

    private static string GetCorrelationId(Microsoft.Extensions.Primitives.StringValues requestedValues)
    {
        if (requestedValues.Count == 1)
        {
            var requested = requestedValues[0];
            if (!string.IsNullOrWhiteSpace(requested)
                && requested.Length <= MaximumLength
                && CorrelationIdPattern().IsMatch(requested))
            {
                return requested;
            }
        }

        return Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
    }

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._-]{0,127}$", RegexOptions.CultureInvariant)]
    private static partial Regex CorrelationIdPattern();
}
