#nullable enable

namespace Modular.Modules.Accounting.Application;

public sealed record Invoice(Guid Id, string Number, string Customer, decimal Amount, DateOnly IssuedOn, string Status);
public sealed record Account(Guid Id, string Code, string Name, decimal Balance);

public interface IAccountingService
{
    IReadOnlyCollection<Invoice> Invoices();
    IReadOnlyCollection<Account> Accounts();
    Invoice CreateInvoice(string number, string customer, decimal amount, DateOnly issuedOn);
}
public sealed class InMemoryAccountingService : IAccountingService
{
    private readonly List<Invoice> _invoices = [];
    private readonly List<Account> _accounts = [new(Guid.NewGuid(), "1000", "Cash", 0), new(Guid.NewGuid(), "2000", "Accounts payable", 0)];
   
    public IReadOnlyCollection<Invoice> Invoices()
    {
        return _invoices.AsReadOnly();
    }

    public IReadOnlyCollection<Account> Accounts()
    {
        return _accounts.AsReadOnly();
    }

    public Invoice CreateInvoice(string number, string customer, decimal amount, DateOnly issuedOn)
    {
        var item = new Invoice(Guid.NewGuid(), number, customer, amount, issuedOn, "Draft");
        _invoices.Add(item); return item;
    }
}

public sealed class AccountingValidator
{
    public IReadOnlyCollection<string> ValidateInvoice(string number, string customer, decimal amount, DateOnly date)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(number))
            errors.Add("Invoice number is required.");

        if (string.IsNullOrWhiteSpace(customer))
            errors.Add("Customer is required.");

        if (amount <= 0)
            errors.Add("Amount must be positive.");

        if (date > DateOnly.FromDateTime(DateTime.UtcNow))
            errors.Add("Invoice date cannot be in the future.");

        return errors;
    }
}
