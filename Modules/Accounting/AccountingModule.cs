#nullable enable
using Modular.Modules.Accounting.Application;
using Modular.Modules.Contracts;

namespace Modular.Modules.Accounting;

public sealed class AccountingModule(ILicenseService license) : IModule
{
    public string Id
    {
        get
        {
            return "accounting";
        }
    }

    public string Name
    {
        get
        {
            return "Accounting";
        }
    }

    public void RegisterServices(IServiceCollection services)
    {
        if (!license.IsEnabled("Accounting"))
            throw new InvalidOperationException("Accounting license is missing or disabled.");
        services.AddSingleton<IAccountingService, InMemoryAccountingService>();
        services.AddSingleton<AccountingValidator>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
    }
}
