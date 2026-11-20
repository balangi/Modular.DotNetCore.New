namespace Modular.Web.Support;

public sealed record ModuleDto(string Id, string Name, string Description, bool IsEnabled, string RequiredLicense);
public sealed record LicenseSnapshot(IReadOnlySet<string> Features);
public sealed record MenuItem(string Text, string Url, string Icon, string? RequiredModule = null, string? RequiredLicense = null);
public sealed record PanelLayout(string Title, IReadOnlyList<MenuItem> Items);

public interface IModuleService
{
    IReadOnlyList<ModuleDto> List();
    ModuleDto? Find(string id);
    ModuleDto Create(string id, string name, string description, string requiredLicense);
    bool Update(string id, string name, string description, string requiredLicense);
    bool SetEnabled(string id, bool enabled);
    bool Delete(string id);
}
public interface ILicenseService { LicenseSnapshot Current(); bool HasFeature(string feature); }
public interface IAuthenticationService
{
    Task<MockUser?> ValidateAsync(string userName, string password, CancellationToken cancellationToken = default);
}
public sealed record MockUser(string UserName, string DisplayName, string[] Roles, string[] Features);

public sealed class InMemoryModuleService : IModuleService
{
    private readonly List<ModuleDto> _items =
    [
        new("Accounting", "Accounting", "Accounting workspace", true, "Accounting"),
        new("Federation", "Sports Federation", "Federation workspace", true, "SportsFederation"),
        new("Reports", "Reports", "Cross-module reports", false, "")
    ];
    public IReadOnlyList<ModuleDto> List() => _items.ToList();
    public ModuleDto? Find(string id) => _items.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    public ModuleDto Create(string id, string name, string description, string requiredLicense) { var m = new ModuleDto(id.Trim(), name.Trim(), description.Trim(), true, requiredLicense.Trim()); _items.Add(m); return m; }
    public bool Update(string id, string name, string description, string requiredLicense) { var i = _items.FindIndex(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)); if (i < 0) return false; _items[i] = _items[i] with { Name = name.Trim(), Description = description.Trim(), RequiredLicense = requiredLicense.Trim() }; return true; }
    public bool SetEnabled(string id, bool enabled) { var i = _items.FindIndex(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)); if (i < 0) return false; _items[i] = _items[i] with { IsEnabled = enabled }; return true; }
    public bool Delete(string id) { var item = Find(id); return item is not null && _items.Remove(item); }
}
public sealed class InMemoryLicenseService : ILicenseService
{
    private readonly LicenseSnapshot _license = new(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Accounting", "SportsFederation" });
    public LicenseSnapshot Current() => _license;
    public bool HasFeature(string feature) => _license.Features.Contains(feature);
}
public sealed class MockAuthenticationService : IAuthenticationService
{
    private static readonly MockUser[] Users =
    [
        new("admin", "System Administrator", ["Administrator"], ["Accounting", "SportsFederation"]),
        new("accounting", "Accounting User", ["User"], ["Accounting"]),
        new("federation", "Federation User", ["User"], ["SportsFederation"])
    ];
    public Task<MockUser?> ValidateAsync(string userName, string password, CancellationToken cancellationToken = default) => Task.FromResult(password == "Pass123!" ? Users.FirstOrDefault(x => x.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase)) : null);
}

public static class MockIdentityDefaults { public const string Scheme = "ModularCookie"; }
