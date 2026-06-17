using System.Net;
using System.Text.Json;
using UnitConversionAPI.Exceptions;

namespace UnitConversionAPI.Middleware;

/// <summary>
/// Global exception-handling middleware that converts domain exceptions into
/// RFC 7807 ProblemDetails JSON responses, keeping controllers free of
/// try/catch boilerplate and ensuring a consistent error shape across the API.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnitNotFoundException ex)
        {
            _logger.LogWarning(ex, "Unit not found: {UnitKey}", ex.UnitKey);
            await WriteProblemAsync(context, HttpStatusCode.UnprocessableEntity,
                "Unit Not Found", ex.Message);
        }
        catch (IncompatibleUnitsException ex)
        {
            _logger.LogWarning(ex, "Incompatible units: {From} -> {To}", ex.FromUnit, ex.ToUnit);
            await WriteProblemAsync(context, HttpStatusCode.UnprocessableEntity,
                "Incompatible Units", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context, HttpStatusCode statusCode, string title, string detail)
    {
        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type     = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status   = (int)statusCode,
            detail,
            traceId  = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
