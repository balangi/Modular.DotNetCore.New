#nullable enable
using Modular.Core.AccessControl;

namespace Modular.Application.Abstractions;
/// <summary>Request-scoped identity and authorization claims supplied by the host.</summary>
public interface ICurrentUserContext
{
    /// <summary>True when a principal has authenticated.</summary>
    bool IsAuthenticated { get; }
    /// <summary>Current user identifier, when available.</summary>
    string? UserId { get; }
    /// <summary>Current user display or login name.</summary>
    string? UserName { get; }
    /// <summary>Current tenant identifier, when applicable.</summary>
    string? TenantId { get; }
    /// <summary>Checks a role without exposing ClaimsPrincipal.</summary>
    bool IsInRole(string role);
    /// <summary>Checks a permission without exposing the identity provider.</summary>
    bool HasPermission(string permission);
    AccessLevel AccessLevel { get; }
    IReadOnlySet<string> Roles { get; }
    IReadOnlySet<string> Permissions { get; }
}
