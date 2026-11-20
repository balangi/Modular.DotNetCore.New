using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Modular.Web.Support;

namespace Modular.Web.Authorization;

public sealed record ModuleRequirement(string ModuleId, string LicenseFeature) : IAuthorizationRequirement;
public sealed class ModuleAuthorizationHandler(IModuleService modules, ILicenseService licenses) : AuthorizationHandler<ModuleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ModuleRequirement requirement)
    {
        var module = modules.Find(requirement.ModuleId);
        if (module?.IsEnabled == true && licenses.HasFeature(requirement.LicenseFeature) && (context.User.Identity?.IsAuthenticated == true)) context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public sealed class ModuleAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("Module:", StringComparison.OrdinalIgnoreCase))
        {
            var parts = policyName.Split(':', 3);
            if (parts.Length == 3)
                return Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().AddRequirements(new ModuleRequirement(parts[1], parts[2])).Build());
        }
        return base.GetPolicyAsync(policyName);
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class ModuleAuthorizeAttribute : AuthorizeAttribute
{
    public ModuleAuthorizeAttribute(string moduleId, string licenseFeature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(licenseFeature);

        ModuleId = moduleId;
        LicenseFeature = licenseFeature;
        Policy = $"Module:{moduleId}:{licenseFeature}";
    }

    public string ModuleId { get; }
    public string LicenseFeature { get; }
}
