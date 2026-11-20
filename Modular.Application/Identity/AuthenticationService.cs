#nullable enable
using Modular.Application.Identity.DTOs;
using Modular.Core.Common;

namespace Modular.Application.Identity;

/// <summary>Coordinates authentication while remaining independent of ASP.NET Core.</summary>
public sealed class AuthenticationService(IIdentityGateway identity)
{
    public async Task<Result<AuthenticationResponse>> SignInAsync(LoginRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password)) 
            return Result<AuthenticationResponse>.Failure("invalid_credentials", "User name and password are required.");

        var response = await identity.SignInAsync(request, ct).ConfigureAwait(false);
        
        return response is null ? Result<AuthenticationResponse>.Failure("invalid_credentials", "The supplied credentials are invalid.") : Result<AuthenticationResponse>.Success(response);
    }

    public async Task<Result> SignOutAsync(CancellationToken ct = default) { await identity.SignOutAsync(ct).ConfigureAwait(false); return Result.Success(); }
}
