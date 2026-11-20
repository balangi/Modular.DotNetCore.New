#nullable enable
using Modular.Modules.Contracts;
namespace Modular.Modules.ModuleManagement;
public sealed class ModuleManagementModule:IModule { public ModuleDescriptor Descriptor { get; }=new("module-management","Module Management","1.0.0","Discover, configure, enable, disable, and remove application modules.",ModuleKind.System,Array.Empty<string>()); }
