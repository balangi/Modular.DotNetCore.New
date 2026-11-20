#nullable enable
namespace Modular.Application.Identity.DTOs;
/// <summary>Sign-in input.</summary>
public sealed record LoginRequest(string UserName, string Password, bool RememberMe = false);
/// <summary>Successful authentication result.</summary>
public sealed record AuthenticationResponse(string UserId, string UserName, string? DisplayName, string AccessToken, DateTimeOffset ExpiresAt);
/// <summary>Safe user projection.</summary>
public sealed record UserDto(string Id, string UserName, string? Email, string? DisplayName, bool IsActive, IReadOnlyCollection<string> Roles);
/// <summary>User mutation input.</summary>
public sealed record SaveUserRequest(string UserName, string? Email, string? DisplayName, string? Password, bool IsActive, IReadOnlyCollection<string> Roles);
/// <summary>Safe role projection.</summary>
public sealed record RoleDto(string Id, string Name, string? Description, IReadOnlyCollection<string> Permissions);
/// <summary>Role mutation input.</summary>
public sealed record SaveRoleRequest(string Name, string? Description, IReadOnlyCollection<string> Permissions);
/// <summary>Identity-provider boundary for DI.</summary>
public interface IIdentityGateway
{
    Task<AuthenticationResponse?> SignInAsync(LoginRequest request, CancellationToken ct = default);
    Task SignOutAsync(CancellationToken ct = default);
    Task<UserDto?> FindUserAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> ListUsersAsync(CancellationToken ct = default);
    Task<bool> UserNameExistsAsync(string name, string? excludingId = null, CancellationToken ct = default);
    Task<UserDto> CreateUserAsync(SaveUserRequest request, CancellationToken ct = default);
    Task<UserDto?> UpdateUserAsync(string id, SaveUserRequest request, CancellationToken ct = default);
    Task<bool> DeleteUserAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<RoleDto>> ListRolesAsync(CancellationToken ct = default);
    Task<RoleDto> CreateRoleAsync(SaveRoleRequest request, CancellationToken ct = default);
    Task<RoleDto?> UpdateRoleAsync(string id, SaveRoleRequest request, CancellationToken ct = default);
    Task<bool> DeleteRoleAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(string userId, CancellationToken ct = default);
}
