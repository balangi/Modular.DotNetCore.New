using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Modular.Web.Support;

using IAuthenticationService = Modular.Web.Support.IAuthenticationService;

namespace Modular.Web.Controllers;

public sealed class AccountController(IAuthenticationService authentication) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await authentication.ValidateAsync(model.UserName, model.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }

        var claims = new List<Claim> {
            new(ClaimTypes.Name,
            user.UserName),
            new("display_name",
            user.DisplayName)
        };
        claims.AddRange(user.Roles.Select(x => new Claim(ClaimTypes.Role, x)));
        claims.AddRange(user.Features.Select(x => new Claim("feature", x)));
        await HttpContext.SignInAsync(MockIdentityDefaults.Scheme, new ClaimsPrincipal(new ClaimsIdentity(claims, MockIdentityDefaults.Scheme)));
        return LocalRedirect(string.IsNullOrWhiteSpace(model.ReturnUrl)
            ? "/Public/Home/Index"
            : model.ReturnUrl
            );
    }

    [HttpGet]
    public IActionResult SelectLevel(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectLevel(LoginViewModel model)
    {
        return await Login(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(MockIdentityDefaults.Scheme);
        return RedirectToAction("Index", "Home", new { area = "Public" });
    }
}

public sealed class LoginViewModel
{
    [System.ComponentModel.DataAnnotations.Required]
    public string UserName { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
