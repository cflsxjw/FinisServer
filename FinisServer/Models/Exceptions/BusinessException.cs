namespace FinisServer.Models.Exceptions;

public class BusinessException(string? message = null) : FinisException(message)
{
    public override int StatusCode { get; init; } = StatusCodes.Status400BadRequest;
}