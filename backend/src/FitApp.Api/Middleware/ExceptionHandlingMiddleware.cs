using System.Net;
using System.Text.Json;
using FitApp.Application.Common.Exceptions;

namespace FitApp.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title, errors) = ex switch
        {
            AppValidationException v => (HttpStatusCode.BadRequest, v.Message, v.Errors),
            NotFoundException => (HttpStatusCode.NotFound, ex.Message, null),
            ConflictException => (HttpStatusCode.Conflict, ex.Message, null),
            ForbiddenException => (HttpStatusCode.Forbidden, ex.Message, null),
            UnauthorizedException => (HttpStatusCode.Unauthorized, ex.Message, null),
            _ => (HttpStatusCode.InternalServerError, "Beklenmeyen bir hata oluştu.", null)
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Yakalanmamış hata");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var payload = JsonSerializer.Serialize(new
        {
            status = (int)status,
            title,
            errors
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        await context.Response.WriteAsync(payload);
    }
}
