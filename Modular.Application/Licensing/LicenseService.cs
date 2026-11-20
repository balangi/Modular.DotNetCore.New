#nullable enable

using Modular.Application.Abstractions;
using Modular.Core.Common;
using Modular.Core.Enums;

namespace Modular.Application.Licensing;

public sealed class LicenseService(
    ILicenseRepository repository,
    ILicenseValidator validator,
    ICurrentUserContext currentUser, // تزریق کانتکست کاربر
    TimeProvider? timeProvider = null)
{
    private readonly ILicenseRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ILicenseValidator _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly ICurrentUserContext _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<Result<LicenseRecord>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        // ارسال TenantId از کانتکست جاری به ریپازیتوری
        var license = await _repository.GetCurrentAsync(_currentUser.TenantId, cancellationToken).ConfigureAwait(false);

        if (license is null)
            return Result<LicenseRecord>.Failure("license.not_found", "No license is installed.");

        var now = _timeProvider.GetUtcNow();
        if (license.Status != LicenseStatus.Active)
            return Result<LicenseRecord>.Failure("license.inactive", $"License status is {license.Status}.");

        if (license.ValidFrom > now)
            return Result<LicenseRecord>.Failure("license.not_yet_valid", "The license validity period has not started.");

        if (license.ValidUntil is { } end && end <= now)
            return Result<LicenseRecord>.Failure("license.expired", "The license has expired.");

        var validation = await _validator.ValidateAsync(license, now, cancellationToken).ConfigureAwait(false);
        return validation.IsValid ? Result<LicenseRecord>.Success(license) : Result<LicenseRecord>.Failure("license.validation_failed", validation.FailureReason ?? "License validation failed.");
    }
}
