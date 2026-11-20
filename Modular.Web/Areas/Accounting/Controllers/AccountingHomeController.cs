#nullable enable
using Microsoft.AspNetCore.Mvc;
using Modular.Modules.Accounting.Application;
using Modular.Web.Authorization;

namespace Modular.Modules.Accounting.Controllers;

//[ApiController]
//[Route("accounting/accounts")]
[Area("Accounting"), ModuleAuthorize("Accounting", "Accounting")]
public sealed class AccountingHomeController(IAccountingService service) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(service.Accounts());
    }
}
