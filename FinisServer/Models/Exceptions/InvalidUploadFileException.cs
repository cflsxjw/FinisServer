namespace FinisServer.Models.Exceptions;

public class InvalidUploadFileException(string message) : FinisException(message)
{
    public override int StatusCode { get; init; } = StatusCodes.Status400BadRequest;
}