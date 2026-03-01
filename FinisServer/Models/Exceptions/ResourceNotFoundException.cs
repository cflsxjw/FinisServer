namespace FinisServer.Models.Exceptions;

public class ResourceNotFoundException(string? message = null) : FinisException(message)
{
    public override int StatusCode { get; init; } = StatusCodes.Status404NotFound;
}