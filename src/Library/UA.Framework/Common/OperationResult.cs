namespace UA.Framework.Common;
public sealed record OperationResult(bool Succeeded, string Message)
{
    public static OperationResult Ok(string message = "Completed") => new(true, message);
    public static OperationResult Fail(string message) => new(false, message);
}
public sealed record OperationResult<T>(bool Succeeded, string Message, T? Data)
{
    public static OperationResult<T> Ok(T data) => new(true, "Completed", data);
    public static OperationResult<T> Fail(string message) => new(false, message, default);
}
