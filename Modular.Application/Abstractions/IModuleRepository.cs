#nullable enable

using Modular.Application.Navigation.DTOs;
using Modular.Core.Common;
using Modular.Core.Modules;

namespace Modular.Application.Abstractions;

public interface IModuleRepository
{
    Task<Module?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Module?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<List<Module>> ListAsync(bool enabledOnly = false, CancellationToken cancellationToken = default);
    Task AddAsync(Module module, CancellationToken cancellationToken = default);
    void Update(Module module);
    void Remove(Module module);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // اضافه شده برای پشتیبانی از منوها در MenuService
    Task<IReadOnlyList<MenuDefinition>> GetMenusAsync(string? moduleKey, CancellationToken ct = default);

    Task<IReadOnlyList<PanelLayoutDefinition>> GetPanelLayoutsAsync(string panelKey, CancellationToken ct = default);
    Task<Result> UpsertPanelLayoutAsync(PanelLayoutDefinition layout, CancellationToken ct = default);
}
