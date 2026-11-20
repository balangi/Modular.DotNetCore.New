using Microsoft.AspNetCore.Identity;

namespace Modular.Infrastructure.Runtime.Identity;

/// <summary>Identity user with tenant scoping and optional multi-level login metadata.</summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? TenantId { get; set; }
    public string? DisplayName { get; set; }
    public string? LoginLevel { get; set; }
    public bool IsSystemUser { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAtUtc { get; set; }
    public string? CustomData { get; set; }
}
