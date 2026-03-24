namespace FinisServer.Models.Exceptions;

public class HttpContextException(string? message = null):FinisException(message)
{
    public override int StatusCode { get; init; } = 500;
}