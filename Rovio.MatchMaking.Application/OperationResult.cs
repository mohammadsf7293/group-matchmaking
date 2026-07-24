namespace Rovio.MatchMaking.Application;

public class OperationResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    public static OperationResult Ok() => new() { Success = true };
    public static OperationResult Fail(string error) => new() { Success = false, Error = error };
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; init; }

    public static OperationResult<T> Ok(T data) => new() { Success = true, Data = data };
    public new static OperationResult<T> Fail(string error) => new() { Success = false, Error = error };
}
