#nullable enable
using Modular.Modules.Contracts;
using Modular.Modules.IdentityManagement.Application;

namespace Modular.Modules.IdentityManagement;

public sealed class IdentityManagementModule : IModule
{
    public string Id
    {
        get
        {
            return "identity-management";
        }
    }

    public string Name
    {
        get
        {
            return "Identity Management";
        }
    }

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IIdentityManagementService, InMemoryIdentityManagementService>();
        services.AddSingleton<IdentityManagementValidator>();
        services.AddSingleton<IAccessService, MockAccessService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
    }
}
