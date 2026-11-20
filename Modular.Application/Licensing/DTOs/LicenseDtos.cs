#nullable enable

using Modular.Core.Enums;

namespace Modular.Application.Licensing.DTOs;

public sealed record LicenseSnapshot(
    string LicenseId,
    string ProductCode,
    string CustomerName,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil,
    LicenseStatus Status,
    IReadOnlySet<string> Features
)
{
    public bool IsValidAt(DateTimeOffset pointInTime) =>
        Status == LicenseStatus.Active && pointInTime >= ValidFrom && (ValidUntil is null || pointInTime <= ValidUntil.Value);

    public bool HasFeature(string featureCode) =>
        Features.Contains(featureCode);
}
