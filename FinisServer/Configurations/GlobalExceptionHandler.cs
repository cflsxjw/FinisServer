using FinisServer.Models;
using FinisServer.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace FinisServer.Configurations;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception is FinisException finisException ? finisException.StatusCode : 500; 
        var message = statusCode == 500 ? "服务器异常" : exception.Message;
        var response = Result.Failure(message);
        Console.WriteLine(exception.Message);
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}