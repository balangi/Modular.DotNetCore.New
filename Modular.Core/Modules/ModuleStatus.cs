using Modular.Core.Enums;

namespace Modular.Core.Modules;

/// <summary>Provides semantic helpers for <see cref="ModuleStatus"/>.</summary>
public static class ModuleStatusExtensions
{
    /// <summary>Returns true only when the module is operational and enabled.</summary>
    public static bool IsEnabled(this ModuleStatus status) => status == ModuleStatus.Running;

    /// <summary>Returns true when the module is in a transitional lifecycle state.</summary>
    public static bool IsTransitioning(this ModuleStatus status) =>
        status is ModuleStatus.Starting or ModuleStatus.Stopping;
}
