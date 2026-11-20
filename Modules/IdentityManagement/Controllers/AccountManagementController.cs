#nullable enable
using Microsoft.AspNetCore.Mvc;
using Modular.Modules.IdentityManagement.Application;
using Modular.Modules.Contracts;

namespace Modular.Modules.IdentityManagement.Controllers;

[ApiController]
[Route("admin/accounts")]
public sealed class AccountManagementController(IIdentityManagementService service, IdentityManagementValidator validator, IAccessService access) : Controller
{
    [HttpGet("users")]
    public IActionResult Users()
    {
        return access.HasPermission("users.read")
            ? Ok(service.Users())
            : Forbid();
    }

    [HttpPost("users")]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        if (!access.HasPermission("users.write"))
            return Forbid();

        var errors = validator.ValidateUser(request.UserName, request.Email, request.AccessLevel);
        return errors.Count > 0
            ? BadRequest(errors)
            : Ok(service.CreateUser(request.UserName, request.Email, request.AccessLevel));
    }

    [HttpGet("roles")]
    public IActionResult Roles()
    {
        return access.HasPermission("roles.read")
            ? Ok(service.Roles())
            : Forbid();
    }

    [HttpPost("roles")]
    public IActionResult CreateRole([FromBody] CreateRoleRequest request)
    {
        if (!access.HasPermission("roles.write"))
            return Forbid();

        var errors = validator.ValidateRole(request.Name, request.Permissions);
        return errors.Count > 0
            ? BadRequest(errors)
            : Ok(service.CreateRole(request.Name, request.Permissions));
    }

    [HttpGet("permissions")]
    public IActionResult Permissions()
    {
        return access.HasPermission("permissions.read") 
            ? Ok(service.Permissions())
            : Forbid();
    }

    public sealed record CreateUserRequest(string UserName, string Email, int AccessLevel); public sealed record CreateRoleRequest(string Name, IReadOnlyCollection<string> Permissions);
}
