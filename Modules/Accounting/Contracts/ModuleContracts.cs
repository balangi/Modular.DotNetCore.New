#nullable enable

namespace Modular.Modules.Contracts;

public interface IModule
{
    string Id { get; }
    string Name { get; }
    void RegisterServices(IServiceCollection services);
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}

public sealed record ModuleDescriptor(string Id, string Name, string Version, bool Enabled, string? RequiredLicense = null);

public interface ILicenseService
{
    bool IsEnabled(string feature);
}

public sealed class MockLicenseService : ILicenseService
{
    public bool IsEnabled(string feature)
    {
        return false;
    }
}

public interface IAccessService
{
    bool HasPermission(string permission);
}

public sealed class MockAccessService : IAccessService
{
    public bool HasPermission(string permission) => true;
}

public sealed class ModuleCatalog
{
    private readonly List<ModuleDescriptor> _items = [];

    public IReadOnlyList<ModuleDescriptor> Modules
    {
        get
        {
            return _items;
        }
    }

    public void Add(ModuleDescriptor descriptor)
    {
        _items.Add(descriptor);
    }
}
