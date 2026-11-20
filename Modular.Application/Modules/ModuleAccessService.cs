#nullable enable

using Modular.Application.Abstractions;
using Modular.Application.Licensing;
using Modular.Core.AccessControl;
using Modular.Core.Enums;
using Modular.Core.Modules;

namespace Modular.Application.Modules;

public sealed record ModuleAccessDecision(bool IsAllowed, string? Reason = null)
{
    public static ModuleAccessDecision Allow { get; } = new(true);
    public static ModuleAccessDecision Deny(string reason) => new(false, reason);
}

/// <summary>Enforces status, dependency, product license, access tier, roles, and permissions.</summary>
public sealed class ModuleAccessService(ModuleRegistry registry, ICurrentUserContext currentUser, LicenseService licenseService)
{
    private readonly ModuleRegistry _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    private readonly ICurrentUserContext _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly LicenseService _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));

    public async Task<ModuleAccessDecision> CheckAsync(string moduleId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
            return ModuleAccessDecision.Deny("A module identifier is required.");

        if (!_registry.TryGet(moduleId.Trim(), out var module) || module is null)
            return ModuleAccessDecision.Deny("Module is not registered.");

        return await CheckTreeAsync(module, new HashSet<Guid>(), cancellationToken).ConfigureAwait(false);
    }

    private async Task<ModuleAccessDecision> CheckTreeAsync(Module module, HashSet<Guid> visited, CancellationToken ct)
    {
        if (!visited.Add(module.Id))
            return ModuleAccessDecision.Deny("A dependency cycle prevents module access.");

        if (module.Status != ModuleStatus.Active || !module.IsEnabled)
            return ModuleAccessDecision.Deny($"Module '{module.Name}' is not active or enabled.");

        // بررسی وابستگی‌ها
        foreach (var dependencyKey in module.Dependencies ?? Array.Empty<string>())
        {
            if (!_registry.TryGet(dependencyKey, out var dependency) || dependency is null)
                return ModuleAccessDecision.Deny($"Required module '{dependencyKey}' is unavailable.");

            var decision = await CheckTreeAsync(dependency, visited, ct).ConfigureAwait(false);
            if (!decision.IsAllowed)
                return ModuleAccessDecision.Deny($"Dependency '{dependencyKey}' is not accessible: {decision.Reason}");
        }

        // بررسی لایسنس
        if (!string.IsNullOrEmpty(module.RequiredProductCode))
        {
            var license = await _licenseService.GetCurrentAsync(ct).ConfigureAwait(false);
            if (license.IsFailure)
                return ModuleAccessDecision.Deny($"A valid product license is required: {license.ErrorMessage}");

            if (!StringComparer.OrdinalIgnoreCase.Equals(license.Value!.ProductCode, module.RequiredProductCode))
                return ModuleAccessDecision.Deny($"The installed license is not for product '{module.RequiredProductCode}'.");
        }

        // بررسی سطح دسترسی با استفاده از Rank
        // فرض: Public کمترین سطح دسترسی است
        if (module.MinimumAccessLevel.Rank > AccessLevel.Public.Rank && !_currentUser.IsAuthenticated)
            return ModuleAccessDecision.Deny("Authentication is required.");

        if (_currentUser.AccessLevel.Rank < module.MinimumAccessLevel.Rank)
            return ModuleAccessDecision.Deny("The user's access level is insufficient.");

        // بررسی نقش‌ها و پرمیشن‌ها
        if (module.AllowedRoles.Count > 0 && !module.AllowedRoles.Any(_currentUser.Roles.Contains))
            return ModuleAccessDecision.Deny("The user does not have an allowed role.");

        if (module.RequiredPermissions.Any(p => !_currentUser.Permissions.Contains(p)))
            return ModuleAccessDecision.Deny("The user is missing one or more required permissions.");

        return ModuleAccessDecision.Allow;
    }
}
