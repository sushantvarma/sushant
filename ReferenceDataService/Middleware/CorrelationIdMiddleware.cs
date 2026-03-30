using Serilog.Context;

namespace ReferenceDataService.Middleware;

/// <summary>
/// Middleware to generate and track correlation IDs for request tracing
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string CorrelationIdPropertyName = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Process the HTTP request and add correlation ID
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or retrieve correlation ID
        var correlationId = GetOrCreateCorrelationId(context);

        // Add correlation ID to Serilog LogContext for structured logging
        using (LogContext.PushProperty(CorrelationIdPropertyName, correlationId))
        {
            _logger.LogInformation(
                "HTTP request received - Method: {HttpMethod}, Path: {Path}, CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            // Add correlation ID to response headers for client reference
            context.Response.Headers.Add(CorrelationIdHeader, correlationId);

            try
            {
                await _next(context);

                _logger.LogInformation(
                    "HTTP response sent - Status: {StatusCode}, CorrelationId: {CorrelationId}",
                    context.Response.StatusCode,
                    correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception occurred in request pipeline - CorrelationId: {CorrelationId}",
                    correlationId);
                throw;
            }
        }
    }

    /// <summary>
    /// Gets correlation ID from request headers or generates a new one
    /// </summary>
    private string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationIdValue))
        {
            var correlationId = correlationIdValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(correlationId))
            {
                return correlationId;
            }
        }

        // Generate new correlation ID if not provided
        var newCorrelationId = $"{context.TraceIdentifier}-{Guid.NewGuid():N}";
        return newCorrelationId;
    }
}
