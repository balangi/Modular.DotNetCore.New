using Microsoft.Extensions.DependencyInjection;
using Modular.Infrastructure.Runtime.Modules;
using Modular.Modules.Accounting.Application;

namespace Modular.Modules.Accounting;

public sealed class AccountingModule : IRuntimeModule
{
    public string Id => "accounting";
    public Version Version => new(1, 3, 0);
    public string Name => "Accounting";

    public IEnumerable<string> Dependencies => Array.Empty<string>();

    public void ConfigureServices(IServiceCollection services)
    {
        // ثبت سرویس‌های ماژول حسابداری
        services.AddSingleton<IAccountingService, InMemoryAccountingService>();
        services.AddSingleton<AccountingValidator>();
    }
}
