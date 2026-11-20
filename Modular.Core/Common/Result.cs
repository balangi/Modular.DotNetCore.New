#nullable enable

namespace Modular.Core.Common;

/// <summary>
/// Represents an error with a required code and message.
/// </summary>
public sealed record ResultError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultError"/> record.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">A description of the error.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> or <paramref name="message"/> is null, empty, or whitespace.
    /// </exception>
    public ResultError(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Error code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Error message is required.", nameof(message));
        }

        Code = code.Trim();
        Message = message.Trim();
    }

    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }
}

/// <summary>
/// Represents the outcome of an operation that either succeeds or fails with one or more errors.
/// </summary>
public class Result
{
    private readonly IReadOnlyList<ResultError> _errors;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">A value indicating whether the operation succeeded.</param>
    /// <param name="errors">The errors associated with a failed operation.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when a successful result contains errors or a failed result contains no errors.
    /// </exception>
    protected Result(bool isSuccess, IEnumerable<ResultError>? errors)
    {
        var errorSnapshot = errors?.ToArray() ?? [];
        _errors = Array.AsReadOnly(errorSnapshot);

        if (isSuccess && _errors.Count > 0)
        {
            throw new ArgumentException("A successful result cannot contain errors.", nameof(errors));
        }

        if (!isSuccess && _errors.Count == 0)
        {
            throw new ArgumentException("A failed result must contain at least one error.", nameof(errors));
        }

        IsSuccess = isSuccess;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the errors associated with this result.
    /// The collection is empty when the operation succeeded.
    /// </summary>
    public IReadOnlyList<ResultError> Errors => _errors;

    /// <summary>
    /// Gets the code of the first error, or <see langword="null"/> when the operation succeeded.
    /// </summary>
    public string? ErrorCode => _errors.Count == 0 ? null : _errors[0].Code;

    /// <summary>
    /// Gets the message of the first error, or <see langword="null"/> when the operation succeeded.
    /// </summary>
    public string? ErrorMessage => _errors.Count == 0 ? null : _errors[0].Message;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result containing one error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public static Result Failure(string code, string message) =>
        new(false, [new ResultError(code, message)]);

    /// <summary>
    /// Creates a failed result containing the specified errors.
    /// </summary>
    /// <param name="errors">The errors associated with the failure.</param>
    public static Result Failure(IEnumerable<ResultError> errors) =>
        new(false, errors);

    /// <summary>
    /// Creates a typed successful result containing the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="value">The value to include in the result.</param>
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    /// <summary>
    /// Creates a typed failed result containing one error.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public static Result<T> Failure<T>(string code, string message) =>
        Result<T>.Failure(code, message);

    /// <summary>
    /// Creates a failed result containing one error (Alias for Failure).
    /// </summary>
    public static Result Error(string code, string message) =>
        Failure(code, message);

    /// <summary>
    /// Creates a failed result containing the specified errors (Alias for Failure).
    /// </summary>
    public static Result Error(IEnumerable<ResultError> errors) =>
        Failure(errors);

    /// <summary>
    /// Creates a typed failed result containing one error (Alias for Failure).
    /// </summary>
    public static Result<T> Error<T>(string code, string message) =>
        Failure<T>(code, message);

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if this result represents a failure.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result contains errors.
    /// </exception>
    public void ThrowIfFailure()
    {
        if (IsFailure)
        {
            var message = string.Join("; ", _errors.Select(error => $"{error.Code}: {error.Message}"));
            throw new InvalidOperationException(message);
        }
    }
}

/// <summary>
/// Represents the outcome of an operation that returns a value when successful, or errors when it fails.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, IEnumerable<ResultError>? errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the value produced by a successful operation, or default when failed.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Creates a successful typed result containing the specified value.
    /// </summary>
    /// <param name="value">The value to include.</param>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Creates a failed typed result containing one error.
    /// </summary>
    public static new Result<T> Failure(string code, string message) =>
        new(false, default, [new ResultError(code, message)]);

    /// <summary>
    /// Creates a failed typed result containing the specified errors.
    /// </summary>
    public static new Result<T> Failure(IEnumerable<ResultError> errors) =>
        new(false, default, errors);

    /// <summary>
    /// Creates a failed typed result containing one error (Alias for Failure).
    /// </summary>
    public static new Result<T> Error(string code, string message) =>
        Failure(code, message);

    /// <summary>
    /// Creates a failed typed result containing the specified errors (Alias for Failure).
    /// </summary>
    public static new Result<T> Error(IEnumerable<ResultError> errors) =>
        Failure(errors);

    /// <summary>
    /// Gets the value of a successful result, or throws if the result failed.
    /// </summary>
    public T GetValueOrThrow()
    {
        ThrowIfFailure();
        return Value!;
    }
}

/// <summary>
/// Represents a lightweight error structure using a code and a message.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Message">The error message.</param>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Gets an empty error value representing no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}
