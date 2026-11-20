#nullable enable

using Modular.Application.Abstractions;
using Modular.Core.Common;
using Modular.Core.Modules;

namespace Modular.Application.Modules;

/// <summary>Maintains an atomically replaced, validated in-memory view of persisted modules.</summary>
public sealed class ModuleRegistry(IModuleRepository repository)
{
    private readonly IModuleRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly SemaphoreSlim _reloadLock = new(1, 1);

    private IReadOnlyDictionary<string, Module> _snapshot = new Dictionary<string, Module>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<Module> Modules
    {
        get
        {
            return Volatile.Read(ref _snapshot).Values.ToArray();
        }
    }

    public bool TryGet(string id, out Module? module) => Volatile.Read(ref _snapshot).TryGetValue(id, out module);

    public async Task<Result<IReadOnlyCollection<Module>>> ReloadAsync(CancellationToken cancellationToken = default)
    {
        await _reloadLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // استفاده از ListAsync طبق اینترفیس IModuleRepository
            var rows = await _repository.ListAsync(false, cancellationToken).ConfigureAwait(false);
            var map = new Dictionary<string, Module>(StringComparer.OrdinalIgnoreCase);

            foreach (var module in rows)
            {
                if (string.IsNullOrWhiteSpace(module.Key))
                    return Result<IReadOnlyCollection<Module>>.Failure("module.invalid_key", "A module has an empty key/identifier.");

                if (!map.TryAdd(module.Key, module))
                    return Result<IReadOnlyCollection<Module>>.Failure("module.duplicate_key", $"Duplicate module key '{module.Key}'.");
            }

            foreach (var module in map.Values)
            {
                foreach (var dependency in module.Dependencies ?? Array.Empty<string>())
                {
                    if (!map.ContainsKey(dependency))
                    {
                        return Result<IReadOnlyCollection<Module>>.Failure("module.missing_dependency", $"Module '{module.Key}' depends on missing module '{dependency}'.");
                    }
                }
            }

            var cycle = FindCycle(map);
            if (cycle is not null)
                return Result<IReadOnlyCollection<Module>>.Failure("module.dependency_cycle", $"Module dependency cycle detected: {cycle}.");

            IReadOnlyDictionary<string, Module> immutable = new Dictionary<string, Module>(map, StringComparer.OrdinalIgnoreCase);
            Volatile.Write(ref _snapshot, immutable);
            return Result<IReadOnlyCollection<Module>>.Success(immutable.Values.ToArray());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyCollection<Module>>.Failure("module.registry_load_failed", ex.Message);
        }
        finally
        {
            _reloadLock.Release();
        }
    }

    private static string? FindCycle(IReadOnlyDictionary<string, Module> map)
    {
        var state = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        var path = new List<string>();
        foreach (var id in map.Keys)
        {
            var found = Visit(id);
            if (found is not null)
                return found;
        }

        return null;

        string? Visit(string id)
        {
            if (state.TryGetValue(id, out var current))
            {
                if (current != 1) return null;
                var start = path.FindIndex(x => StringComparer.OrdinalIgnoreCase.Equals(x, id));
                return string.Join(" -> ", path.Skip(start).Append(id));
            }
            state[id] = 1; path.Add(id);
            foreach (var dep in map[id].Dependencies ?? Array.Empty<string>()) { var found = Visit(dep); if (found is not null) return found; }
            path.RemoveAt(path.Count - 1); state[id] = 2; return null;
        }
    }
}
