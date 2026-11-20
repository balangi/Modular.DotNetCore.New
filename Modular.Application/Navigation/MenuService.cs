#nullable enable

using Modular.Application.Abstractions;
using Modular.Application.Identity;
using Modular.Application.Licensing.DTOs;
using Modular.Application.Navigation.DTOs;

namespace Modular.Application.Navigation;

/// <summary>Builds a module-aware, role/permission/license-filtered menu tree.</summary>
public sealed class MenuService(IModuleRepository modules, MenuPermissionService permissions, ILicenseRepository licenses, ICurrentUserContext currentUser)
{
    public async Task<IReadOnlyList<MenuItemDto>> GetAsync(string? moduleKey = null, CancellationToken ct = default)
    {
        var record = await licenses.GetCurrentAsync(currentUser.TenantId, ct).ConfigureAwait(false);

        // تبدیل LicenseRecord به LicenseSnapshot
        LicenseSnapshot? license = record is null ? null : new(
            record.LicenseId,
            record.ProductCode,
            record.CustomerName,
            record.ValidFrom,
            record.ValidUntil,
            record.Status,
            record.Features
        );

        var definitions = await modules.GetMenusAsync(moduleKey, ct).ConfigureAwait(false);
        var allowed = new List<MenuDefinition>();

        foreach (var item in definitions.Where(x => x.Enabled).OrderBy(x => x.SortOrder))
        {
            if ((await permissions.CanAccessAsync(item.Policy, license, ct).ConfigureAwait(false)).Allowed)
            {
                allowed.Add(item);
            }
        }

        return allowed.Where(x => x.ParentId is null).Select(x => Map(x, allowed)).ToArray();
    }

    private static MenuItemDto Map(MenuDefinition item, IReadOnlyList<MenuDefinition> all)
    {
        return new(item.Id, item.Label, item.Href, item.Icon, all.Where(x => x.ParentId == item.Id).OrderBy(x => x.SortOrder).Select(x => Map(x, all)).ToArray(), item.ModuleKey);
    }
}
