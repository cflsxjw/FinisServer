using System.Net;

namespace FinisServer.Models.Exceptions;

public class AuthenticationException(string? message = null) : FinisException(message)
{
    public override int StatusCode { get; init; } = StatusCodes.Status401Unauthorized;
}