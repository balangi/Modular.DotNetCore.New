#nullable enable
using Modular.Modules.Contracts; namespace Modular.Modules.LicenseManagement;
public sealed class LicenseManagementModule:IModule { public ModuleDescriptor Descriptor {get;}=new("license-management","License Management","1.0.0","Register, activate, bind, and assign features to licenses.",ModuleKind.System,Array.Empty<string>()); }
