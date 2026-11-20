#nullable enable

using System;
using Modular.Core.Common;

namespace Modular.Core.AccessControl;

public sealed class Permission : Entity
{
    public Permission(Guid id, string key, string name, string? description = null, Guid? moduleId = null) : base(id)
    {
        Key = Validate(key, nameof(key), 150);
        Name = Validate(name, nameof(name), 200);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ModuleId = moduleId == Guid.Empty ? null : moduleId;
    }

    public string Key { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Guid? ModuleId { get; }

    public void Rename(string name) { Name = Validate(name, nameof(name), 200); Touch(); }
    public void UpdateDescription(string? description) { Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(); Touch(); }

    private static string Validate(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength) throw new ArgumentOutOfRangeException(parameterName);
        return normalized;
    }
}
