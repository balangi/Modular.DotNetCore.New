using Microsoft.AspNetCore.Mvc; using Modular.Web.Authorization;
namespace Modular.Web.Areas.Accounting.Controllers;
[Area("Accounting"), ModuleAuthorize("Accounting", "Accounting")] public sealed class AccountingHomeController : Controller { public IActionResult Index() => View(); }
