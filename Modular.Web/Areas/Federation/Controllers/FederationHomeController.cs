using Microsoft.AspNetCore.Mvc;
using Modular.Web.Authorization;

namespace Modular.Web.Areas.Federation.Controllers;

[Area("Federation"), ModuleAuthorize("Federation", "SportsFederation")]
public sealed class FederationHomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
