using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Modular.Infrastructure.Runtime.Modules;
using Modular.Web.Authorization;
using Modular.Web.Navigation;
using Modular.Web.Support;
using System.Reflection;
using System.Runtime.Loader;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAuthorizationPolicyProvider, ModuleAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, ModuleAuthorizationHandler>();
builder.Services.AddSingleton<IModuleService, InMemoryModuleService>();
builder.Services.AddSingleton<Modular.Web.Support.ILicenseService, InMemoryLicenseService>();
builder.Services.AddSingleton<IAuthenticationService, MockAuthenticationService>();
builder.Services.AddSingleton<IMenuService, MenuService>();
builder.Services.AddSingleton<IPanelLayoutService, PanelLayoutService>();


// ۱. بارگذاری پویا و کشف ماژول‌ها از پوشه Modules (پیش از ساخت کانتینر DI)
var modulesDirectory = Path.Combine(builder.Environment.ContentRootPath, "Modules");
if (!Directory.Exists(modulesDirectory))
{
    Directory.CreateDirectory(modulesDirectory);
}

var discovery = new ModuleDiscovery();
var discoveredModules = discovery.Discover(new[] { modulesDirectory });

var mvcBuilder = builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// ثبت سرویس‌های ماژول‌های فعال و افزودن ApplicationPart به MVC
foreach (var descriptor in discoveredModules.Where(m => m.IsActive))
{
    try
    {
        var assembly = AssemblyLoadContext.Default
            .LoadFromAssemblyPath(descriptor.AssemblyPath);

        var moduleType = assembly.GetTypes()
            .FirstOrDefault(type =>
                typeof(IRuntimeModule).IsAssignableFrom(type) &&
                type.IsClass &&
                !type.IsAbstract);

        if (moduleType is null)
        {
            Console.WriteLine(
                $"No IRuntimeModule implementation found in: {assembly.FullName}");

            continue;
        }

        if (Activator.CreateInstance(moduleType) is not IRuntimeModule moduleInstance)
        {
            Console.WriteLine(
                $"Could not create module instance: {moduleType.FullName}");

            continue;
        }

        // فراخوانی متد ثبت سرویس‌ها در صورت وجود قرارداد مشترک
        // moduleInstance.RegisterServices(builder.Services);
        moduleInstance.ConfigureServices(builder.Services);

        // افزودن کنترلرها و ویوهای ماژول به موتور MVC
        mvcBuilder.AddApplicationPart(assembly);

        Console.WriteLine(
            $"Loaded module: {moduleInstance.Id} - {moduleInstance.Version}");
    }
    catch (ReflectionTypeLoadException ex)
    {
        var loaderErrors = string.Join(
            Environment.NewLine,
            ex.LoaderExceptions
                .Where(error => error is not null)
                .Select(error => error!.Message));

        Console.WriteLine(
            $"Failed to inspect module assembly '{descriptor.AssemblyPath}':{Environment.NewLine}{loaderErrors}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Failed to load module '{descriptor.Id}': {ex}");
    }
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = MockIdentityDefaults.Scheme;
    options.DefaultChallengeScheme = MockIdentityDefaults.Scheme;
    options.DefaultSignInScheme = MockIdentityDefaults.Scheme;
})
.AddCookie(MockIdentityDefaults.Scheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Public/Home/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("AccountingAccess", policy => policy.AddRequirements(new ModuleRequirement("Accounting", "Accounting")));
    options.AddPolicy("FederationAccess", policy => policy.AddRequirements(new ModuleRequirement("Federation", "SportsFederation")));
});


var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Public/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ۲. سرویس فایل‌های استاتیک ماژول‌ها از پوشه /Modules
var modulesPath = Path.Combine(builder.Environment.ContentRootPath, "Modules");
if (Directory.Exists(modulesPath))
{
    // این خط تمام پوشه‌های wwwroot داخل ماژول‌ها را کشف و با پیشوند /modules سرو می‌کند
    foreach (var moduleDir in Directory.GetDirectories(modulesPath))
    {
        var moduleWwwRoot = Path.Combine(moduleDir, "wwwroot");
        if (Directory.Exists(moduleWwwRoot))
        {
            var moduleName = Path.GetFileName(moduleDir).ToLowerInvariant();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(moduleWwwRoot),
                RequestPath = $"/modules/{moduleName}"
            });
        }
    }
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Account}/{action=Login}/{id?}");

// لاگ نگاری در برنامه
var actionProvider =
    app.Services.GetRequiredService<IActionDescriptorCollectionProvider>();

var logger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("RuntimeInspection");

var actionDescriptors = actionProvider.ActionDescriptors.Items;

logger.LogWarning(
    "Total MVC action descriptors: {Count}",
    actionDescriptors.Count);

Console.Error.WriteLine(
    $"Total MVC action descriptors: {actionDescriptors.Count}");

foreach (var actionDescriptor in actionDescriptors)
{
    if (actionDescriptor is not ControllerActionDescriptor controllerAction)
    {
        continue;
    }

    var controllerType = controllerAction.ControllerTypeInfo;
    var controllerAssembly = controllerType.Assembly;

    var controllerName = controllerAction.ControllerName;
    var actionName = controllerAction.ActionName;
    var area = controllerAction.RouteValues.TryGetValue("area", out var areaValue)
        ? areaValue
        : null;

    var routeText = string.IsNullOrWhiteSpace(area)
        ? $"/{controllerName}/{actionName}"
        : $"/{area}/{controllerName}/{actionName}";

    var message =
        $"MVC ACTION | Route: {routeText} | " +
        $"Controller: {controllerType.FullName} | " +
        $"Assembly: {controllerAssembly.GetName().Name} | " +
        $"Location: {controllerAssembly.Location}";

    logger.LogWarning(message);
    Console.Error.WriteLine(message);
}

var applicationPartManager = app.Services.GetRequiredService<Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager>();

Console.Error.WriteLine("=== MVC APPLICATION PARTS ===");

foreach (var part in applicationPartManager.ApplicationParts)
{
    var partName = part.Name;
    var partType = part.GetType().FullName;

    Console.Error.WriteLine(
        $"ApplicationPart: {partName} | Type: {partType}");
}

app.Run();
