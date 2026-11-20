#nullable enable
using Microsoft.AspNetCore.Mvc;
using Modular.Modules.Accounting.Application;

namespace Modular.Modules.Accounting.Controllers;

[ApiController]
[Route("accounting/invoices")]
public sealed class InvoicesController(IAccountingService service, AccountingValidator validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(service.Invoices());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateInvoiceRequest request)
    {
        var errors = validator.ValidateInvoice(request.Number, request.Customer, request.Amount, request.IssuedOn);
        return errors.Count > 0 
            ? BadRequest(errors) 
            : Ok(service.CreateInvoice(request.Number, request.Customer, request.Amount, request.IssuedOn));
    }

    public sealed record CreateInvoiceRequest(string Number, string Customer, decimal Amount, DateOnly IssuedOn);
}
