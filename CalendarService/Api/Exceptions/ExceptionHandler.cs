using Microsoft.AspNetCore.Diagnostics;

namespace Api.Exceptions;

public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var url = $"{httpContext.Request.Path}{httpContext.Request.QueryString}";

        if (exception is HttpRequestException httpEx)
        {
            logger.LogError(httpEx, "HTTP request failed while calling an external service. URL: {Url}", url);
        }

        else
        {
            logger.LogError(exception, "System error exception occurred. URL: {Url}", url);
        }

        return ValueTask.FromResult(false);
    }
}
