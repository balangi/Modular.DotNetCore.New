namespace Modular.Modules.FederationManagement;

public sealed class FederationManagementModule : Modular.Core.Modules.IModule
{
    public string Id
    {
        get
        {
            return "federation-management";
        }
    }

    public string Name
    {
        get
        {
            return "مدیریت فدراسیون";
        }
    }

    public string RequiredLicenseType
    {
        get
        {
            return "SportsFederation";
        }
    }

    public bool RequiresEnabledLicense
    {
        get
        {
            return true;
        }
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<Application.FederationManagementStore>();
        services.AddScoped<Application.FederationManagementService>();
        services.AddSingleton<Application.FederationManagementValidator>();
    }
}
