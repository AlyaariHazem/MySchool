namespace Backend.Common;

public readonly record struct Result<T>
{
    private Result(bool ok, T? value, string? error)
        => (Ok, Value, Error) = (ok, value, error);

    public bool Ok { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Fail(string message) => new(false, default, message);
}
