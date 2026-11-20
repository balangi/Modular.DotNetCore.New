#nullable enable

using System;
using Modular.Core.Common;

namespace Modular.Core.AccessControl;

public sealed class ModuleAccessRule : Entity
{
    public ModuleAccessRule(Guid id, Guid moduleId, Guid accessLevelId, bool isEnabled = true, DateTimeOffset? validFromUtc = null, DateTimeOffset? validUntilUtc = null) : base(id)
    {
        if (moduleId == Guid.Empty) throw new ArgumentException("Module id is required.", nameof(moduleId));
        if (accessLevelId == Guid.Empty) throw new ArgumentException("Access level id is required.", nameof(accessLevelId));
        if (validFromUtc.HasValue && validUntilUtc.HasValue && validUntilUtc < validFromUtc)
            throw new ArgumentException("Validity end must not precede its start.", nameof(validUntilUtc));
        ModuleId = moduleId;
        AccessLevelId = accessLevelId;
        IsEnabled = isEnabled;
        ValidFromUtc = validFromUtc;
        ValidUntilUtc = validUntilUtc;
    }

    public Guid ModuleId { get; }
    public Guid AccessLevelId { get; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset? ValidFromUtc { get; }
    public DateTimeOffset? ValidUntilUtc { get; }

    public bool IsActiveAt(DateTimeOffset? atUtc = null)
    {
        var at = atUtc ?? DateTimeOffset.UtcNow;
        return IsEnabled && (!ValidFromUtc.HasValue || at >= ValidFromUtc) && (!ValidUntilUtc.HasValue || at <= ValidUntilUtc);
    }

    public void Enable() { IsEnabled = true; Touch(); }
    public void Disable() { IsEnabled = false; Touch(); }
}
