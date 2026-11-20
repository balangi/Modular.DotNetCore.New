#nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Modular.Core.Common;

namespace Modular.Application.Licensing;

/// <summary>Checks features only after the overall product license has been validated.</summary>
public sealed class LicenseFeatureService(LicenseService licenseService)
{
    private readonly LicenseService _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));

    public async Task<Result<bool>> HasFeatureAsync(string featureCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureCode))
        {
            return Result<bool>.Failure("license.invalid_feature", "Feature code is required.");
        }

        var licenseResult = await _licenseService.GetCurrentAsync(cancellationToken).ConfigureAwait(false);

        if (licenseResult.IsFailure)
        {
            // اصلاح: دسترسی به Code و Message از طریق اولین خطا در لیست Errors (با استفاده از Errors.First() یا Errors[0])
            var firstError = licenseResult.Errors.First();
            return Result<bool>.Failure(firstError.Code, firstError.Message);
        }

        var license = licenseResult.Value;
        if (license is null)
        {
            return Result<bool>.Failure("license.not_found", "No active license found.");
        }

        var hasFeature = license.Features.Contains(featureCode.Trim(), StringComparer.OrdinalIgnoreCase);
        return Result<bool>.Success(hasFeature);
    }

    public async Task<Result<bool>> RequireFeatureAsync(string featureCode, CancellationToken cancellationToken = default)
    {
        var result = await HasFeatureAsync(featureCode, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result;
        }

        return result.Value
            ? result
            : Result<bool>.Failure("license.feature_not_entitled", $"The current license does not include feature '{featureCode}'.");
    }
}
