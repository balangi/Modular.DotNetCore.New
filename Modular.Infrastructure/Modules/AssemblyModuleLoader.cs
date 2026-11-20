using System.Reflection;
using System.Runtime.Loader;

namespace Modular.Infrastructure.Runtime.Modules;

/// <summary>Loads modules into collectible AssemblyLoadContexts; activation toggles are runtime state only.</summary>
public sealed class AssemblyModuleLoader : IAsyncDisposable
{
    private readonly Dictionary<string, LoadedModule> _loaded = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();
    private readonly IReadOnlyList<string> _sharedAssemblies;

    public AssemblyModuleLoader(IEnumerable<string>? sharedAssemblyNames = null) =>
        _sharedAssemblies = (sharedAssemblyNames ?? new[] { typeof(IRuntimeModule).Assembly.GetName().Name! }).ToArray();

    public IReadOnlyCollection<string> ActiveModuleIds
    { get { lock (_sync) return _loaded.Values.Where(x => x.Active).Select(x => x.Descriptor.Id).ToArray(); } }

    public IRuntimeModule Load(ModuleDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        lock (_sync)
        {
            if (_loaded.ContainsKey(descriptor.Id)) throw new InvalidOperationException($"Module '{descriptor.Id}' is already loaded.");
            foreach (var dependency in descriptor.Dependencies)
                if (!_loaded.TryGetValue(dependency, out var dep) || !dep.Active)
                    throw new InvalidOperationException($"Required module '{dependency}' is not loaded and active.");
            var path = Path.GetFullPath(descriptor.AssemblyPath);
            if (!File.Exists(path)) throw new FileNotFoundException("Module assembly not found.", path);
            var context = new PluginLoadContext(path, _sharedAssemblies);
            try
            {
                var assembly = context.LoadFromAssemblyPath(path);
                var type = assembly.GetType(descriptor.ModuleType.FullName!, throwOnError: true, ignoreCase: false)!;
                if (!typeof(IRuntimeModule).IsAssignableFrom(type)) throw new InvalidOperationException("Module type does not implement IRuntimeModule; ensure the contract assembly is shared.");
                if (Activator.CreateInstance(type) is not IRuntimeModule instance) throw new InvalidOperationException("Module requires a public parameterless constructor.");
                if (!string.Equals(instance.Id, descriptor.Id, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Discovered and loaded module IDs differ.");
                _loaded.Add(descriptor.Id, new LoadedModule(descriptor, context, instance, descriptor.IsActive));
                return instance;
            }
            catch { context.Unload(); throw; }
        }
    }

    public void SetActive(string moduleId, bool active)
    {
        lock (_sync)
        {
            if (!_loaded.TryGetValue(moduleId, out var item)) throw new KeyNotFoundException($"Module '{moduleId}' is not loaded.");
            if (!active && _loaded.Values.Any(x => x.Active && x.Descriptor.Dependencies.Contains(moduleId, StringComparer.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Cannot deactivate a module while an active dependent module requires it.");
            if (active)
                foreach (var dependency in item.Descriptor.Dependencies)
                    if (!_loaded.TryGetValue(dependency, out var dep) || !dep.Active) throw new InvalidOperationException($"Dependency '{dependency}' is inactive.");
            item.Active = active;
        }
    }

    public async ValueTask UnloadAsync(string moduleId)
    {
        LoadedModule item;
        lock (_sync)
        {
            if (!_loaded.TryGetValue(moduleId, out item!)) return;
            if (_loaded.Values.Any(x => x.Descriptor.Dependencies.Contains(moduleId, StringComparer.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Unload dependent modules first.");
            _loaded.Remove(moduleId);
        }
        if (item.Instance is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        else if (item.Instance is IDisposable disposable) disposable.Dispose();
        item.Context.Unload();
    }

    public async ValueTask DisposeAsync()
    {
        string[] ids; lock (_sync) ids = _loaded.Keys.ToArray();
        foreach (var id in ids.Reverse()) await UnloadAsync(id).ConfigureAwait(false);
    }

    private sealed class LoadedModule(ModuleDescriptor descriptor, PluginLoadContext context, IRuntimeModule instance, bool active)
    {
        public ModuleDescriptor Descriptor { get; } = descriptor;
        public PluginLoadContext Context { get; } = context;
        public IRuntimeModule Instance { get; } = instance;
        public bool Active { get; set; } = active;
    }

    private sealed class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;
        private readonly HashSet<string> _shared;
        public PluginLoadContext(string pluginPath, IEnumerable<string> shared) : base(Path.GetFileNameWithoutExtension(pluginPath), isCollectible: true)
        { _resolver = new AssemblyDependencyResolver(pluginPath); _shared = new HashSet<string>(shared, StringComparer.OrdinalIgnoreCase); }
        protected override Assembly? Load(AssemblyName name)
        {
            if (name.Name is not null && _shared.Contains(name.Name))
                return Default.Assemblies.FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), name));
            var path = _resolver.ResolveAssemblyToPath(name);
            return path is null ? null : LoadFromAssemblyPath(path);
        }
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            return path is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(path);
        }
    }
}
