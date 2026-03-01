namespace FinisServer.Models.Exceptions;

public abstract class FinisException(string? message = null) : Exception(message)
{
    public abstract int StatusCode { get; init; }
}