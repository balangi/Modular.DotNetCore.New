#nullable enable

using Modular.Core.Enums;

namespace Modular.Application.Abstractions;

public interface ILicenseRepository
{
    // اضافه شدن tenantId اختیاری برای هماهنگی با MenuService
    Task<LicenseRecord?> GetCurrentAsync(string? tenantId = null, CancellationToken cancellationToken = default);
}

public sealed record LicenseRecord(string LicenseId, string ProductCode, string CustomerName,
    DateTimeOffset ValidFrom, DateTimeOffset? ValidUntil, LicenseStatus Status,
    IReadOnlySet<string> Features, string? Signature = null);

public sealed record LicenseValidation(bool IsValid, string? FailureReason = null)
{
    public static LicenseValidation Valid { get; } = new(true);
    public static LicenseValidation Invalid(string reason) => new(false, reason);
}
