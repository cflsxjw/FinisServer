namespace FinisServer.Models;

public record Result(string? Message, bool IsSuccess)
{
    public static Result Success(string? msg = null) 
        => new(msg, true);
    public static Result Failure(string? msg = null) 
        => new(msg, false);
}

public record Result<T>(T Data, string? Message, bool IsSuccess)
    : Result(Message, IsSuccess)
{
    public static Result<T> Success(T data, string? msg = null) 
        => new(data, msg, true);
}
    