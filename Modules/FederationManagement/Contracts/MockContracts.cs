using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Modular.Core.Modules
{
    public interface IModule
    {
        string Id { get; }
        string Name { get; }
        string RequiredLicenseType { get; }
        bool RequiresEnabledLicense { get; }
        void ConfigureServices(IServiceCollection services);
    }
}

namespace Modular.Modules.FederationManagement
{
    public interface IFederationLicense
    {
        Task<bool> IsEnabledAsync(string licenseType, CancellationToken cancellationToken);
    }

    public sealed class FederationLicenseGuardFilter(IFederationLicense license) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext c, ActionExecutionDelegate next)
        {
            if (!await license.IsEnabledAsync("SportsFederation", c.HttpContext.RequestAborted))
            {
                c.Result = new ObjectResult(new { error = "لایسنس فعال فدراسیون لازم است." })
                {
                    StatusCode = 403
                }; 
                
                return;
            }
            await next();
        }
    }
}
