namespace Modular.Core.Licensing;

/// <summary>
/// Describes one feature granted by a license.
/// </summary>
public sealed record LicenseFeature
{
    public LicenseFeature(string code, bool isEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("A feature code is required.", nameof(code));
        }

        Code = code.Trim();
        IsEnabled = isEnabled;
    }

    public string Code { get; }

    public bool IsEnabled { get; }

    public static LicenseFeature Enabled(string code) => new(code, true);

    public static LicenseFeature Disabled(string code) => new(code, false);
}
