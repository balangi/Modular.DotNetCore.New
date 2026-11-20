using Modular.Web.Support;
namespace Modular.Web.Navigation;
public interface IPanelLayoutService { PanelLayout Get(string panel, System.Security.Claims.ClaimsPrincipal user); }
public sealed class PanelLayoutService(IMenuService menus) : IPanelLayoutService
{
    public PanelLayout Get(string panel, System.Security.Claims.ClaimsPrincipal user) => new(panel, menus.GetFor(user));
}
