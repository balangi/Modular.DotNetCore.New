using Microsoft.AspNetCore.Identity;

namespace Modular.Infrastructure.Runtime.Identity;

/// <summary>Tenant-aware role with a hierarchical access level.</summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public Guid? TenantId { get; set; }
    public string? DisplayName { get; set; }
    public int AccessLevel { get; set; }
    public bool IsSystemRole { get; set; }
    public string? CustomData { get; set; }
}
