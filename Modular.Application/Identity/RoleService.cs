#nullable enable
using Modular.Application.Identity.DTOs;
using Modular.Core.Common;

namespace Modular.Application.Identity;

/// <summary>Role and permission administration use cases.</summary>
public sealed class RoleService(IIdentityGateway identity)
{
    public Task<IReadOnlyList<RoleDto>> ListAsync(CancellationToken ct = default) => identity.ListRolesAsync(ct);
    public async Task<Result<RoleDto>> CreateAsync(SaveRoleRequest request, CancellationToken ct = default) => string.IsNullOrWhiteSpace(request.Name) ? Result<RoleDto>.Failure("invalid_role", "Role name is required.") : Result<RoleDto>.Success(await identity.CreateRoleAsync(request, ct).ConfigureAwait(false));
    public async Task<Result<RoleDto>> UpdateAsync(string id, SaveRoleRequest request, CancellationToken ct = default) { var role = await identity.UpdateRoleAsync(id, request, ct).ConfigureAwait(false); return role is null ? Result<RoleDto>.Failure("not_found", "Role was not found.") : Result<RoleDto>.Success(role); }
    public async Task<Result> DeleteAsync(string id, CancellationToken ct = default) => await identity.DeleteRoleAsync(id, ct).ConfigureAwait(false) ? Result.Success() : Result.Failure("not_found", "Role was not found.");
}
