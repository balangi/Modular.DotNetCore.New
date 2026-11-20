#nullable enable

using Modular.Application.Abstractions;
using Modular.Application.Identity.DTOs;
using Modular.Application.Licensing.DTOs;
using Modular.Application.Navigation.DTOs;

namespace Modular.Application.Identity;

/// <summary>Central authorization decision service for navigation policies.</summary>
// اصلاح نام اینترفیس از ICurrentUser به ICurrentUserContext
public sealed class MenuPermissionService(ICurrentUserContext currentUser, IIdentityGateway identity)
{
    private readonly ICurrentUserContext _currentUser = currentUser;
    private readonly IIdentityGateway _identity = identity;

    public async Task<AccessDecision> CanAccessAsync(NavigationPolicy policy, LicenseSnapshot? license, CancellationToken ct = default)
    {
        if (policy.Public)
            return new(true, null);

        // استفاده از _currentUser اصلاح شده
        if (!_currentUser.IsAuthenticated)
            return new(false, "authentication_required");

        if (policy.Roles.Count > 0 && !policy.Roles.Any(_currentUser.IsInRole))
            return new(false, "role_required");

        // فرض بر اینکه IIdentityGateway در اسمبلی دیگر است، مطمئن شوید using آن وجود دارد
        var granted = policy.Permissions.Count == 0
            ? Array.Empty<string>()
            : await _identity.GetUserPermissionsAsync(_currentUser.UserId ?? string.Empty, ct).ConfigureAwait(false);

        if (policy.Permissions.Count > 0 && !policy.Permissions.Any(granted.Contains))
            return new(false, "permission_required");

        if (policy.RequiredFeature is not null && (license is null || !license.IsValidAt(DateTimeOffset.UtcNow) || !license.HasFeature(policy.RequiredFeature)))
            return new(false, "license_feature_required");

        return new(true, null);
    }
}

/// <summary>Authorization result.</summary>
public sealed record AccessDecision(bool Allowed, string? Reason);
