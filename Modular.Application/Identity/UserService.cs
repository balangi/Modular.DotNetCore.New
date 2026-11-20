#nullable enable
using Modular.Application.Identity.DTOs;
using Modular.Core.Common;

namespace Modular.Application.Identity;

/// <summary>User administration use cases.</summary>
public sealed class UserService(IIdentityGateway identity)
{
    public Task<UserDto?> GetAsync(string id, CancellationToken ct = default) => identity.FindUserAsync(id, ct);

    public Task<IReadOnlyList<UserDto>> ListAsync(CancellationToken ct = default) => identity.ListUsersAsync(ct);

    public async Task<Result<UserDto>> CreateAsync(SaveUserRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserName)) return Result<UserDto>.Failure("invalid_user", "User name is required.");
        if (await identity.UserNameExistsAsync(request.UserName, ct: ct).ConfigureAwait(false)) return Result<UserDto>.Failure("duplicate_user", "User name is already in use.");
        return Result<UserDto>.Success(await identity.CreateUserAsync(request, ct).ConfigureAwait(false));
    }

    public async Task<Result<UserDto>> UpdateAsync(string id, SaveUserRequest request, CancellationToken ct = default) { var user = await identity.UpdateUserAsync(id, request, ct).ConfigureAwait(false); return user is null ? Result<UserDto>.Failure("not_found", "User was not found.") : Result<UserDto>.Success(user); }

    public async Task<Result> DeleteAsync(string id, CancellationToken ct = default) => await identity.DeleteUserAsync(id, ct).ConfigureAwait(false) ? Result.Success() : Result.Failure("not_found", "User was not found.");
}
