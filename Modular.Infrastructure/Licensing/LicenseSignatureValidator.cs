using System.Security.Cryptography;
using System.Text.Json;

namespace Modular.Infrastructure.Runtime.Licensing;

public sealed record LicensePayload(string LicenseId, string? HardwareId, string? Domain,
    DateTimeOffset ExpiresAtUtc, string[]? Features = null);
public sealed record LicenseValidationResult(bool IsValid, string? Error, LicensePayload? License = null)
{
    public static LicenseValidationResult Valid(LicensePayload license) => new(true, null, license);
    public static LicenseValidationResult Invalid(string reason) => new(false, reason);
}

/// <summary>Verifies signed JSON license tokens: base64url(payload).base64url(ECDSA-SHA256 signature).</summary>
public sealed class LicenseSignatureValidator : IDisposable
{
    private readonly ECDsa _publicKey;
    private readonly JsonSerializerOptions _json;

    public LicenseSignatureValidator(string subjectPublicKeyInfoBase64)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectPublicKeyInfoBase64);
        _publicKey = ECDsa.Create();
        try { _publicKey.ImportSubjectPublicKeyInfo(Convert.FromBase64String(subjectPublicKeyInfoBase64), out _); }
        catch { _publicKey.Dispose(); throw; }
        _json = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public LicenseValidationResult Validate(string token, string? hardwareId, string? domain,
        TimeProvider? timeProvider = null, TimeSpan? clockSkew = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token)) return LicenseValidationResult.Invalid("License token is empty.");
            var parts = token.Split('.');
            if (parts.Length != 2) return LicenseValidationResult.Invalid("Malformed license token.");
            var payloadBytes = FromBase64Url(parts[0]);
            var signature = FromBase64Url(parts[1]);
            if (!_publicKey.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256))
                return LicenseValidationResult.Invalid("Signature verification failed.");
            var license = JsonSerializer.Deserialize<LicensePayload>(payloadBytes, _json);
            if (license is null || string.IsNullOrWhiteSpace(license.LicenseId))
                return LicenseValidationResult.Invalid("License payload is invalid.");
            var now = (timeProvider ?? TimeProvider.System).GetUtcNow();
            if (license.ExpiresAtUtc + (clockSkew ?? TimeSpan.Zero) < now)
                return LicenseValidationResult.Invalid("License has expired.");
            if (!Matches(license.HardwareId, hardwareId)) return LicenseValidationResult.Invalid("Hardware ID mismatch.");
            if (!MatchesDomain(license.Domain, domain)) return LicenseValidationResult.Invalid("Domain mismatch.");
            return LicenseValidationResult.Valid(license);
        }
        catch (Exception ex) when (ex is FormatException or JsonException or CryptographicException or ArgumentException or OverflowException)
        { return LicenseValidationResult.Invalid($"Invalid license: {ex.Message}"); }
    }

    private static bool Matches(string? expected, string? actual) =>
        string.IsNullOrWhiteSpace(expected) || string.Equals(expected.Trim(), actual?.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool MatchesDomain(string? expected, string? actual)
    {
        if (string.IsNullOrWhiteSpace(expected)) return true;
        if (!Uri.TryCreate("https://" + expected.Trim(), UriKind.Absolute, out var expectedUri) ||
            !Uri.TryCreate("https://" + (actual ?? "").Trim(), UriKind.Absolute, out var actualUri)) return false;
        return string.Equals(expectedUri.IdnHost, actualUri.IdnHost, StringComparison.OrdinalIgnoreCase);
    }

    private static byte[] FromBase64Url(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded += new string('=', (4 - padded.Length % 4) % 4);
        return Convert.FromBase64String(padded);
    }
    public void Dispose() => _publicKey.Dispose();
}
