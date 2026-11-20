using Modular.Core.Enums;

namespace Modular.Core.Licensing;

/// <summary>
/// Represents an issued license and the features it grants.
/// </summary>
public sealed class License
{

    public License(
        Guid id,
        LicenseType licenseType,
        DateTimeOffset? expiryDate,
        IEnumerable<LicenseFeature> features,
        string? customerId = null,
        DateTimeOffset? issuedAt = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A license identifier is required.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(features);

        var issued = issuedAt ?? DateTimeOffset.UtcNow;
        var featureList = features.ToArray();

        if (featureList.Any(static feature => feature is null))
        {
            throw new ArgumentException("Features cannot contain null values.", nameof(features));
        }

        if (featureList
            .GroupBy(static feature => feature.Code, StringComparer.OrdinalIgnoreCase)
            .Any(static group => group.Count() > 1))
        {
            throw new ArgumentException("Feature codes must be unique.", nameof(features));
        }

        if (expiryDate is not null && expiryDate < issued)
        {
            throw new ArgumentException(
                "The expiry date cannot be earlier than the issue date.",
                nameof(expiryDate));
        }

        Id = id;
        LicenseType = licenseType;
        CustomerId = string.IsNullOrWhiteSpace(customerId) ? null : customerId.Trim();
        IssuedDate = issued;
        ExpiryDate = expiryDate;
        Features = Array.AsReadOnly(featureList);
    }

    public Guid Id { get; }

    public LicenseType LicenseType { get; }

    public string? CustomerId { get; }

    public DateTimeOffset IssuedDate { get; }

    /// <summary>
    /// The instant at which the license expires. A null value means no expiry.
    /// </summary>
    public DateTimeOffset? ExpiryDate { get; }

    public IReadOnlyList<LicenseFeature> Features { get; }
    public string LicenseKey { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public LicenseProductType ProductType { get; private set; }
    public LicenseStatus Status { get; private set; }
    public string? Signature { get; private set; }

    public bool IsExpired(DateTimeOffset? at = null) =>
        ExpiryDate is not null && ExpiryDate <= (at ?? DateTimeOffset.UtcNow);

    public bool HasFeature(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return Features.Any(feature =>
            feature.IsEnabled &&
            string.Equals(feature.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public LicenseValidationResult Validate(DateTimeOffset? at = null)
    {
        var validationTime = at ?? DateTimeOffset.UtcNow;
        var errors = new List<string>();

        if (LicenseType == LicenseType.Unknown)
        {
            errors.Add("The license type is not specified.");
        }

        if (ExpiryDate is not null && ExpiryDate <= validationTime)
        {
            errors.Add("The license has expired.");
        }

        return errors.Count == 0
            ? LicenseValidationResult.Success()
            : LicenseValidationResult.Failure(errors.ToArray());
    }
}

    
