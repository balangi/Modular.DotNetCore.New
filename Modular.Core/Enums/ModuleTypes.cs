namespace Modular.Core.Enums;

/// <summary>Lifecycle and deployment state of a module.</summary>
public enum ModuleStatus
{
    Active = 0,
    /// <summary>The module has been discovered but not installed.</summary>
    Discovered = 1,

    /// <summary>The module is installed and can be started.</summary>
    Installed = 2,

    /// <summary>The module is currently starting.</summary>
    Starting = 3,

    /// <summary>The module is running and enabled.</summary>
    Running = 4,

    /// <summary>The module is currently stopping.</summary>
    Stopping = 5,

    /// <summary>The module is installed but explicitly disabled.</summary>
    Disabled = 6,

    /// <summary>The module failed to start or encountered an unrecoverable runtime error.</summary>
    Failed = 7,

    /// <summary>The module was removed from the deployment.</summary>
    Uninstalled = 8,
    Unavailable = 9
}

/// <summary>Describes whether a module is generally applicable or tied to a product.</summary>
public enum ModuleType
{
    // OtherTyps: Core, Feature, Integration 

    /// <summary>A module that is not restricted to a specific product.</summary>
    General = 0,

    /// <summary>A module whose availability is restricted by a product code and license.</summary>
    ProductSpecific = 1,

    Core = 2,
    Feature = 3,
    Integration = 4
}

