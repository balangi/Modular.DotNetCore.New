#nullable enable

using Modular.Core.Common;

namespace Modular.Core.AccessControl;

public sealed class Role : Entity
{
    private readonly HashSet<Guid> _permissionIds = [];
    private readonly HashSet<Guid> _accessLevelIds = [];

    public Role(Guid id, string key, string name, string? description = null, bool isSystemRole = false) : base(id)
    {
        Key = Validate(key, nameof(key), 150);
        Name = Validate(name, nameof(name), 200);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsSystemRole = isSystemRole;
    }

    public string Key { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Guid Id { get; private set; }
    public string NormalizedName { get; private set; } = string.Empty;
    public AccessLevel AccessLevel { get; private set; }
    public bool IsSystemRole { get; }
    public IReadOnlySet<Guid> PermissionIds => _permissionIds;
    public IReadOnlySet<Guid> AccessLevelIds => _accessLevelIds;

    public void Rename(string name) { Name = Validate(name, nameof(name), 200); Touch(); }
    public void UpdateDescription(string? description) { Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(); Touch(); }

    public bool GrantPermission(Guid permissionId)
    {
        EnsureId(permissionId, nameof(permissionId));
        var added = _permissionIds.Add(permissionId);
        if (added) Touch();
        return added;
    }

    public bool RevokePermission(Guid permissionId)
    {
        var removed = _permissionIds.Remove(permissionId);
        if (removed) Touch();
        return removed;
    }

    public bool GrantAccessLevel(Guid accessLevelId)
    {
        EnsureId(accessLevelId, nameof(accessLevelId));
        var added = _accessLevelIds.Add(accessLevelId);
        if (added) Touch();
        return added;
    }

    public bool RevokeAccessLevel(Guid accessLevelId)
    {
        var removed = _accessLevelIds.Remove(accessLevelId);
        if (removed) Touch();
        return removed;
    }

    public bool HasPermission(Guid permissionId) => _permissionIds.Contains(permissionId);
    public bool HasAccessLevel(Guid accessLevelId) => _accessLevelIds.Contains(accessLevelId);

    private static void EnsureId(Guid id, string parameterName)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.", parameterName);
    }

    private static string Validate(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength) throw new ArgumentOutOfRangeException(parameterName);
        return normalized;
    }
}
