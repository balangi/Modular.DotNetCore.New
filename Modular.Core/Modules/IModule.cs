namespace Modular.Core.Modules;

/// <summary>Defines the minimal contract implemented by a discoverable module.</summary>
public interface IModule
{
    /// <summary>Gets the immutable metadata that identifies and describes the module.</summary>
    ModuleDescriptor Descriptor { get; }
}
