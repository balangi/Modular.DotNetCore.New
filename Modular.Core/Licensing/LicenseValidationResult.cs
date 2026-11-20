namespace Modular.Core.Licensing;

/// <summary>
/// Represents the outcome of license validation.
/// </summary>
public sealed class LicenseValidationResult
{
    private LicenseValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public bool IsValid { get; }

    public bool IsInvalid => !IsValid;

    public IReadOnlyList<string> Errors { get; }

    public static LicenseValidationResult Success() =>
        new(true, Array.Empty<string>());

    public static LicenseValidationResult Failure(params string[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var normalizedErrors = errors
            .Where(static error => !string.IsNullOrWhiteSpace(error))
            .Select(static error => error.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedErrors.Length == 0)
        {
            throw new ArgumentException("At least one validation error is required.", nameof(errors));
        }

        return new(false, normalizedErrors);
    }
}
