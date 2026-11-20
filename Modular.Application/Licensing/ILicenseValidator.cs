#nullable enable
using Modular.Application.Abstractions;

namespace Modular.Application.Licensing;

/// <summary>Host/infrastructure implementation should perform cryptographic signature verification.</summary>
public interface ILicenseValidator
{
    Task<LicenseValidation> ValidateAsync(LicenseRecord license, DateTimeOffset now, CancellationToken cancellationToken = default);
}
