using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modular.Web.Authorization;
using Modular.Web.Navigation;
using Modular.Web.Support;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddAuthentication(MockIdentityDefaults.Scheme).AddCookie(options =>
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

builder.Services.AddSingleton<IAuthorizationPolicyProvider, ModuleAuthorizationPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, ModuleAuthorizationHandler>();
builder.Services.AddSingleton<IModuleService, InMemoryModuleService>();
builder.Services.AddSingleton<ILicenseService, InMemoryLicenseService>();
builder.Services.AddSingleton<IAuthenticationService, MockAuthenticationService>();
builder.Services.AddSingleton<IMenuService, MenuService>();
builder.Services.AddSingleton<IPanelLayoutService, PanelLayoutService>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Public/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Account}/{action=Login}/{id?}");

app.Run();
