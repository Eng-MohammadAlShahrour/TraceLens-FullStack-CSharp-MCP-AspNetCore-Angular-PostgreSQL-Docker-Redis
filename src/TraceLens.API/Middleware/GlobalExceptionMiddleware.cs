using System.Net;
using System.Text.Json;
using FluentValidation;
using TraceLens.Application.Common.Exceptions;

namespace TraceLens.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (ValidationException vex)
        {
            _logger.LogWarning("Validation failed: {Errors}", vex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "ValidationError",
                title = "One or more validation errors occurred.",
                status = 400,
                errors
            }));
        }
        catch (DuplicateWorkOrderException dex)
        {
            _logger.LogWarning("Duplicate work order: {Message}", dex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "DuplicateWorkOrder",
                title = dex.Message,
                status = 409
            }));
        }
        catch (ArgumentException aex)
        {
            _logger.LogWarning("Argument error: {Message}", aex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "BadRequest",
                title = aex.Message,
                status = 400
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "ServerError",
                title = "An unexpected error occurred.",
                status = 500
            }));
        }
    }
}
