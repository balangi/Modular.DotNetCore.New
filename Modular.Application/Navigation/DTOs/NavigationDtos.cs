#nullable enable

namespace Modular.Application.Navigation.DTOs;

/// <summary>Role, permission, public-access, and license policy.</summary>
public sealed record NavigationPolicy(bool Public, IReadOnlyCollection<string> Roles, IReadOnlyCollection<string> Permissions, string? RequiredFeature)
{
    public static NavigationPolicy PublicAccess { get; } = new(true, Array.Empty<string>(), Array.Empty<string>(), null);
}

/// <summary>Persisted menu definition.</summary>
public sealed record MenuDefinition(string Id, string Label, string? Href, string? Icon, int SortOrder, string? ModuleKey, string? ParentId, NavigationPolicy Policy, bool Enabled = true);

/// <summary>Authorized menu projection.</summary>
public sealed record MenuItemDto(string Id, string Label, string? Href, string? Icon, IReadOnlyList<MenuItemDto> Children, string? ModuleKey);

/// <summary>Administrator-managed panel layout.</summary>
public sealed record PanelLayoutDefinition(string PanelKey, string LayoutJson, int Version, DateTimeOffset UpdatedAt, string? UpdatedBy);

/// <summary>Resolved panel layout.</summary>
public sealed record PanelLayoutDto(string PanelKey, string LayoutJson, int Version, bool IsDefault);

/// <summary>Persisted module metadata.</summary>
public sealed record ModuleRecord(string Key, string Name, bool Enabled, bool PublicAccess, string? RequiredFeature, IReadOnlyCollection<string> RequiredPermissions);
