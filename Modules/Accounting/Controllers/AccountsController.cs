#nullable enable
using Microsoft.AspNetCore.Mvc;
using Modular.Modules.Accounting.Application;

namespace Modular.Modules.Accounting.Controllers;

[ApiController]
[Route("accounting/accounts")]
public sealed class AccountsController(IAccountingService service) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(service.Accounts());
    }
}
