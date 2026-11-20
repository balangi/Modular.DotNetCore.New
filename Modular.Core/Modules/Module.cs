#nullable enable

using Modular.Core.AccessControl;
using Modular.Core.Enums;

namespace Modular.Core.Modules;

public sealed class Module
{
    private Module() { }

    public Module(Guid id, string key, string name, string version, ModuleType type = ModuleType.Feature)
    {
        Id = id;
        Key = key;
        Name = name;
        Version = version;
        Type = type;
        Status = ModuleStatus.Active;
        IsEnabled = true;
        CreatedUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ModuleType Type { get; private set; }
    public ModuleStatus Status { get; private set; }
    public bool IsEnabled { get; private set; }

    // پراپرتی‌های جدید اضافه شده برای هماهنگی با Service
    public ICollection<string> Dependencies { get; private set; } = new List<string>();
    public string? RequiredProductCode { get; set; }
    public AccessLevel MinimumAccessLevel { get; set; } = AccessLevel.Anonymous;
    public ICollection<string> AllowedRoles { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public ICollection<string> RequiredPermissions { get; private set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public DateTimeOffset CreatedUtc { get; private set; }
    public DateTimeOffset? UpdatedUtc { get; private set; }
    public ICollection<Menu> Menus { get; private set; } = new List<Menu>();

    public void Update(string name, string? description, ModuleStatus status,
    IEnumerable<string> dependencies, string? productCode,
    AccessLevel accessLevel, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        Name = name;
        Description = description;
        Status = status;
        Dependencies = dependencies.ToList();
        RequiredProductCode = productCode;
        MinimumAccessLevel = accessLevel;
        AllowedRoles = roles.ToList();
        RequiredPermissions = permissions.ToList();
        UpdatedUtc = DateTimeOffset.UtcNow;
    }
}
