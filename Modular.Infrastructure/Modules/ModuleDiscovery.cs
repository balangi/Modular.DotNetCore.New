using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.Loader;

namespace Modular.Infrastructure.Runtime.Modules;

public interface IRuntimeModule
{
    string Id { get; }
    Version Version { get; }
    IEnumerable<string> Dependencies => Array.Empty<string>();
    void ConfigureServices(IServiceCollection services);
}

public sealed record ModuleDescriptor(string Id, Version Version, string AssemblyPath,
    Type ModuleType, bool IsActive, IReadOnlyList<string> Dependencies);

/// <summary>Scans plugin folders and optional already-loaded assemblies without instantiating modules.</summary>
public sealed class ModuleDiscovery
{
    public IReadOnlyList<ModuleDescriptor> Discover(IEnumerable<string> directories,
        IEnumerable<Assembly>? assemblyCatalog = null, ISet<string>? activeModuleIds = null)
    {
        ArgumentNullException.ThrowIfNull(directories);
        var assemblies = new List<(Assembly Assembly, string Path)>();
        foreach (var directory in directories.Where(Directory.Exists).Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
            foreach (var path in Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
                try { assemblies.Add((AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(path)), path)); }
                catch (BadImageFormatException) { /* Native or unrelated DLL. */ }
                catch (FileLoadException) { /* Already loaded or incompatible; catalog may supply it. */ }
        if (assemblyCatalog is not null)
            assemblies.AddRange(assemblyCatalog.Where(a => a is not null).Select(a => (a, a.Location)));

        var found = new Dictionary<string, ModuleDescriptor>(StringComparer.OrdinalIgnoreCase);
        foreach (var (assembly, path) in assemblies)
        {
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t is not null).Cast<Type>().ToArray(); }
            foreach (var type in types.Where(t => !t.IsAbstract && !t.IsInterface && typeof(IRuntimeModule).IsAssignableFrom(t)))
            {
                if (type.GetConstructor(Type.EmptyTypes) is null) continue;
                try
                {
                    var module = (IRuntimeModule)Activator.CreateInstance(type)!;
                    if (string.IsNullOrWhiteSpace(module.Id)) continue;
                    var descriptor = new ModuleDescriptor(module.Id, module.Version, path, type,
                        activeModuleIds is null || activeModuleIds.Contains(module.Id), module.Dependencies.ToArray());
                    if (!found.TryAdd(descriptor.Id, descriptor))
                        throw new InvalidOperationException($"Duplicate module ID '{descriptor.Id}'.");
                }
                catch (TargetInvocationException ex) { throw new InvalidOperationException($"Could not inspect module '{type.FullName}'.", ex.InnerException ?? ex); }
            }
        }
        ValidateDependencies(found);
        return found.Values.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void ValidateDependencies(IReadOnlyDictionary<string, ModuleDescriptor> modules)
    {
        foreach (var module in modules.Values)
            foreach (var dependency in module.Dependencies)
                if (!modules.ContainsKey(dependency)) throw new InvalidOperationException($"Module '{module.Id}' requires missing module '{dependency}'.");
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var done = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Visit(string id)
        {
            if (done.Contains(id)) return;
            if (!visiting.Add(id)) throw new InvalidOperationException($"Module dependency cycle detected at '{id}'.");
            foreach (var dep in modules[id].Dependencies) Visit(dep);
            visiting.Remove(id); done.Add(id);
        }
        foreach (var id in modules.Keys) Visit(id);
    }
}
