using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Shorty.Exceptions;

namespace Shorty.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public virtual async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not CustomBaseException)
        {
            var requestAsText = await RequestAsTextAsync(httpContext);
            _logger.LogError(exception, "{RequestAsText}{NewLine1}{NewLine2}{ExceptionMessage}", requestAsText, Environment.NewLine, Environment.NewLine, exception.Message);
        }

        HttpStatusCode statusCode = DecideStatusCode(exception);
        var payload = DecidePayload(exception);

        var errorHttpContentStr = JsonSerializer.Serialize(payload);

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(errorHttpContentStr, cancellationToken: cancellationToken);

        return true;
    }

    private static HttpStatusCode DecideStatusCode(Exception exception)
    {
        return exception switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            ValidationException => HttpStatusCode.BadRequest,
            BusinessException => HttpStatusCode.UnprocessableEntity,
            _ => HttpStatusCode.InternalServerError
        };
    }

    private static object DecidePayload(Exception exception)
    {
        object payload = exception switch
        {
            ValidationException validationException => new ValidationErrorResponse("Request validation error. Please look for `ErrorMessages` for details.", validationException.ValidationErrors),
            CustomBaseException baseException => new ErrorResponse(baseException.Message),
            _ => new { Message = "Unexpected error occurs" }
        };

        return payload;
    }

    private static async Task<string> RequestAsTextAsync(HttpContext httpContext)
    {
        var rawRequestBody = await GetRawBodyAsync(httpContext.Request);

        IEnumerable<string> headerLine = httpContext.Request
            .Headers
            .Where(h => h.Key != "Authentication")
            .Select(pair => $"{pair.Key} => {string.Join("|", pair.Value.ToList())}");
        var headerText = string.Join(Environment.NewLine, headerLine);

        var message =
            $"Request: {httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}{Environment.NewLine}" +
            $"Headers: {Environment.NewLine}{headerText}{Environment.NewLine}" +
            $"Content : {Environment.NewLine}{rawRequestBody}";

        return message;
    }

    private static async Task<string> GetRawBodyAsync(HttpRequest request, Encoding? encoding = null)
    {
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);
        var body = await reader.ReadToEndAsync().ConfigureAwait(false);
        request.Body.Position = 0;

        return body;
    }

    public record ErrorResponse(string Message);

    public record ValidationErrorResponse(string Message, List<ValidationError> ErrorMessages)
        : ErrorResponse(Message);
}