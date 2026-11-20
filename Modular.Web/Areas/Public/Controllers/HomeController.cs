using Microsoft.AspNetCore.Mvc;

namespace Modular.Web.Areas.Public.Controllers;

[Area("Public")] 
public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult UnauthorizedModule(string module)
    {
        return View(model: module);
    }

    public IActionResult Error()
    {
        return View();
    }
}
