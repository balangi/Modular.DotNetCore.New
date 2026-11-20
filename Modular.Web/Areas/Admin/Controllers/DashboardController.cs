using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modular.Web.Support;

namespace Modular.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Administrator")]
public sealed class DashboardController(IModuleService modules) : Controller
{
    public IActionResult Index()
    {
        return View(modules.List());
    }
}
