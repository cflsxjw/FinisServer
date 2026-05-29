namespace FinisServer.Models.Exceptions;

public class OperationNotAllowedException(string message) : FinisException(message)
{
    public override int StatusCode { get; init; } = StatusCodes.Status403Forbidden;
}