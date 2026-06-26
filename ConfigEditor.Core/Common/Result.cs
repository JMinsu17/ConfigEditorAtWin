using System;

namespace ConfigEditor.Core.Common;

/// <summary>
/// Represents the result of an operation, indicating success or failure.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ErrorInfo? Error { get; }

    protected Result(bool isSuccess, ErrorInfo? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("A successful result cannot have an error.");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(ErrorInfo error) => new(false, error);
    public static Result Failure(string code, string message) => new(false, new ErrorInfo(code, message));
}

/// <summary>
/// Represents the result of an operation that returns a value on success.
/// </summary>
/// <typeparam name="T">The type of value.</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access the value of a failure result.");

    private Result(bool isSuccess, T? value, ErrorInfo? error) : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public new static Result<T> Failure(ErrorInfo error) => new(false, default, error);
    public new static Result<T> Failure(string code, string message) => new(false, default, new ErrorInfo(code, message));

    public static implicit operator Result<T>(T value) => Success(value);
}
