using Modular.Web.Support;
namespace Modular.Web.Navigation;
public interface IMenuService { IReadOnlyList<MenuItem> GetFor(System.Security.Claims.ClaimsPrincipal user); }
public sealed class MenuService(IModuleService modules, ILicenseService licenses) : IMenuService
{
    public IReadOnlyList<MenuItem> GetFor(System.Security.Claims.ClaimsPrincipal user) => new[]
    {
        new MenuItem("Home", "/Public/Home/Index", "home"),
        new MenuItem("Admin", "/Admin/Dashboard/Index", "settings"),
        new MenuItem("Accounting", "/Accounting/AccountingHome/Index", "calculator", "Accounting", "Accounting"),
        new MenuItem("Federation", "/Federation/FederationHome/Index", "trophy", "Federation", "SportsFederation")
    }.Where(x => x.RequiredModule is null || (modules.Find(x.RequiredModule)?.IsEnabled == true && licenses.HasFeature(x.RequiredLicense!))).ToList();
}
