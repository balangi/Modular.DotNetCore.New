#nullable enable
using Microsoft.EntityFrameworkCore;
using Modular.Application.Abstractions;
using Modular.Application.Navigation.DTOs;
using Modular.Core.Common; // برای Result
using Modular.Core.Enums;
using Modular.Core.Modules;

namespace Modular.Infrastructure.Persistence.Repositories;

public sealed class ModuleRepository(ModularDbContext dbContext) : IModuleRepository
{
    public Task<Module?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Modules.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Module?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        dbContext.Modules.AsNoTracking().SingleOrDefaultAsync(x => x.Key == key, cancellationToken);

    public Task<List<Module>> ListAsync(bool enabledOnly = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Module> query = dbContext.Modules.AsNoTracking().OrderBy(x => x.Name);
        // اگر ModuleStatus در فضای‌نام Modular.Core.Modules یا Enums باشد، اینجا بدون خطا کار می‌کند
        if (enabledOnly) query = query.Where(x => x.IsEnabled && x.Status == ModuleStatus.Active).OrderBy(x => x.Name);
        return query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Module module, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(module);
        await dbContext.Modules.AddAsync(module, cancellationToken);
    }

    public void Update(Module module)
    {
        ArgumentNullException.ThrowIfNull(module);
        dbContext.Modules.Update(module);
    }

    public void Remove(Module module)
    {
        ArgumentNullException.ThrowIfNull(module);
        dbContext.Modules.Remove(module);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    // پیاده‌سازی متدهای منو و چیدمان پنل:
    public async Task<IReadOnlyList<MenuDefinition>> GetMenusAsync(
        string? moduleKey,
        CancellationToken ct = default)
    {
        var menus = await dbContext.Menus
            .AsNoTracking()
            .ToListAsync(ct);

        // IMPORTANT:
        // نام propertyهای Title / Url / ModuleId / Enabled را مطابق Menu.cs خودتان جایگزین کنید.
        // چون خطاهای فعلی ثابت می‌کنند Menu دارای ModuleKey, Label, Href, IsEnabled نیست.

        return menus
            // اگر Menu دارای ModuleId است، باید کلید ماژول را از رابطه‌ی Module بخوانید.
            // .Where(x => string.IsNullOrWhiteSpace(moduleKey) || x.Module.Key == moduleKey)

            .Select(m => new MenuDefinition(
                Id: m.Id.ToString(),

                // این propertyها را طبق مدل Menu خودتان اصلاح کنید:
                Label: m.Title,
                Href: m.Route,
                Icon: m.Icon,
                SortOrder: m.SortOrder,

                // اگر Menu به Module وصل است، معمولاً چیزی شبیه این است:
                ModuleKey: null,

                ParentId: m.ParentId?.ToString(),

                Policy: NavigationPolicy.PublicAccess,

                // احتمالاً نام property شما Enabled یا IsVisible است، نه IsEnabled:
                Enabled: true
            ))
            .ToList();
    }

    public async Task<IReadOnlyList<PanelLayoutDefinition>> GetPanelLayoutsAsync(string panelKey, CancellationToken ct = default)
    {
        // فرض بر این است که PanelLayouts در DbContext وجود دارد؛ اگر نام DbSet متفاوت است آن را اصلاح کنید
        var layouts = await dbContext.Set<PanelLayoutEntity>()
            .AsNoTracking()
            .Where(p => p.PanelKey == panelKey)
            .ToListAsync(ct);

        return layouts.Select(p => new PanelLayoutDefinition(
            p.PanelKey,
            p.LayoutJson,
            p.Version,
            p.UpdatedAt,
            p.UpdatedBy
        )).ToList();
    }

    public async Task<Result> UpsertPanelLayoutAsync(PanelLayoutDefinition layout, CancellationToken ct = default)
    {
        var dbSet = dbContext.Set<PanelLayoutEntity>();
        var existing = await dbSet.FirstOrDefaultAsync(p => p.PanelKey == layout.PanelKey, ct);

        if (existing == null)
        {
            var entity = new PanelLayoutEntity
            {
                PanelKey = layout.PanelKey,
                LayoutJson = layout.LayoutJson,
                Version = layout.Version,
                UpdatedAt = layout.UpdatedAt,
                UpdatedBy = layout.UpdatedBy
            };
            await dbSet.AddAsync(entity, ct);
        }
        else
        {
            existing.LayoutJson = layout.LayoutJson;
            existing.Version = layout.Version;
            existing.UpdatedAt = layout.UpdatedAt;
            existing.UpdatedBy = layout.UpdatedBy;
            dbSet.Update(existing);
        }

        await dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// اگر موجودیت PanelLayoutEntity به صورت مجزا تعریف شده باشد:
public class PanelLayoutEntity
{
    public string PanelKey { get; set; } = default!;
    public string LayoutJson { get; set; } = default!;
    public int Version { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
