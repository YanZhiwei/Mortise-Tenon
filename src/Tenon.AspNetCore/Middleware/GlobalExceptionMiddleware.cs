using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tenon.Serialization.Abstractions;

namespace Tenon.AspNetCore.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ISerializer serializer,
    ILogger<GlobalExceptionMiddleware> logger)
{
    protected readonly ILogger<GlobalExceptionMiddleware> Logger = logger;
    protected readonly RequestDelegate Next = next;
    protected readonly ISerializer Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));


    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await Next(context);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An unhandled exception occurred: {Message}\nStack Trace: {StackTrace}", ex.Message,
                ex.StackTrace);
            await HandleExceptionAsync(context, ex);
        }
    }

    protected virtual Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Title = "An error occurred while processing your request",
            Status = context.Response.StatusCode,
            Detail = exception.Message,
            Instance = context.Request.Path,
            Extensions =
            {
                { "timestamp", DateTime.UtcNow.ToString("o") },
                { "correlationId", context.TraceIdentifier }
            }
        };

        var result = Serializer.SerializeObject(problemDetails);
        return context.Response.WriteAsync(result);
    }
}