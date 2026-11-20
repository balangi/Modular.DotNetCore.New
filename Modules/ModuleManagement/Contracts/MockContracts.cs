#nullable enable
using System.Collections.Concurrent;
using System.Reflection;
namespace Modular.Modules.Contracts;
public interface IModule { ModuleDescriptor Descriptor { get; } }
public sealed record ModuleDescriptor(string Id,string Name,string Version,string Description,ModuleKind Kind,IReadOnlyCollection<string> Dependencies);
public enum ModuleKind { System, Feature, Integration }
public enum ModuleStatus { Discovered, Enabled, Disabled, Failed }
public sealed record ModuleRecord(string Id,string Name,string Version,string Description,ModuleKind Kind,ModuleStatus Status,bool IsDiscovered,IReadOnlyCollection<string> Dependencies,DateTimeOffset? LastChangedUtc);
public interface IModuleCatalog { IReadOnlyCollection<ModuleRecord> List(); ModuleRecord? Find(string id); ModuleRecord Upsert(ModuleRecord module); bool Remove(string id); }
public sealed class InMemoryModuleCatalog:IModuleCatalog { readonly ConcurrentDictionary<string,ModuleRecord> items=new(StringComparer.OrdinalIgnoreCase); public IReadOnlyCollection<ModuleRecord> List()=>items.Values.OrderBy(x=>x.Name).ToArray(); public ModuleRecord? Find(string id)=>items.TryGetValue(id,out var x)?x:null; public ModuleRecord Upsert(ModuleRecord x)=>items.AddOrUpdate(x.Id,x,(_,_)=>x); public bool Remove(string id)=>items.TryRemove(id,out _); }
public sealed record LicenseFeature(string Code,string Name,bool Enabled);
public enum LicenseStatus { Pending, Active, Suspended, Expired, Revoked }
public sealed record LicenseRecord(Guid Id,string Key,string Product,string Customer,string? TenantId,DateTimeOffset IssuedUtc,DateTimeOffset ExpiresUtc,LicenseStatus Status,IReadOnlyCollection<LicenseFeature> Features);
public interface ILicenseCatalog { IReadOnlyCollection<LicenseRecord> List(); LicenseRecord? Find(Guid id); LicenseRecord? FindByKey(string key); LicenseRecord Upsert(LicenseRecord license); }
public sealed class InMemoryLicenseCatalog:ILicenseCatalog { readonly ConcurrentDictionary<Guid,LicenseRecord> items=new(); public IReadOnlyCollection<LicenseRecord> List()=>items.Values.OrderByDescending(x=>x.IssuedUtc).ToArray(); public LicenseRecord? Find(Guid id)=>items.TryGetValue(id,out var x)?x:null; public LicenseRecord? FindByKey(string key)=>items.Values.FirstOrDefault(x=>string.Equals(x.Key,key,StringComparison.OrdinalIgnoreCase)); public LicenseRecord Upsert(LicenseRecord x)=>items.AddOrUpdate(x.Id,x,(_,_)=>x); }
public static class ModuleDiscovery { public static IEnumerable<ModuleDescriptor> Discover(Assembly a)=>a.GetTypes().Where(t=>!t.IsAbstract&&typeof(IModule).IsAssignableFrom(t)).Select(t=>Activator.CreateInstance(t) as IModule).Where(x=>x is not null).Select(x=>x!.Descriptor); }
