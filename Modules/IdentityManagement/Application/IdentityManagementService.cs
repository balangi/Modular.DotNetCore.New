#nullable enable

namespace Modular.Modules.IdentityManagement.Application;

public sealed record UserRecord(Guid Id, string UserName, string Email, bool IsActive, int AccessLevel);

public sealed record RoleRecord(Guid Id, string Name, IReadOnlyCollection<string> Permissions);

public interface IIdentityManagementService
{
    IReadOnlyCollection<UserRecord> Users();
    IReadOnlyCollection<RoleRecord> Roles();
    IReadOnlyCollection<string> Permissions();
    UserRecord CreateUser(string userName, string email, int level);
    RoleRecord CreateRole(string name, IReadOnlyCollection<string> permissions);
}

public sealed class InMemoryIdentityManagementService : IIdentityManagementService
{
    private readonly List<UserRecord> _users = [];
    private readonly List<RoleRecord> _roles = [];
    private readonly string[] _permissions = ["users.read", "users.write", "roles.read", "roles.write", "permissions.read", "permissions.write"];

    public IReadOnlyCollection<UserRecord> Users()
    {
        return _users.AsReadOnly();
    }

    public IReadOnlyCollection<RoleRecord> Roles()
    {
        return _roles.AsReadOnly();
    }

    public IReadOnlyCollection<string> Permissions()
    {
        return _permissions;
    }

    public UserRecord CreateUser(string userName, string email, int level)
    {
        var u = new UserRecord(Guid.NewGuid(), userName, email, true, level);
        _users.Add(u); return u;
    }

    public RoleRecord CreateRole(string name, IReadOnlyCollection<string> permissions)
    {
        var r = new RoleRecord(Guid.NewGuid(), name, permissions); _roles.Add(r);
        return r;
    }
}
public sealed class IdentityManagementValidator
{
    public IReadOnlyCollection<string> ValidateUser(string userName, string email, int level)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(userName))
            errors.Add("User name is required.");

        if (!System.Net.Mail.MailAddress.TryCreate(email, out _))
            errors.Add("A valid email is required.");

        if (level is < 0 or > 5)
            errors.Add("Access level must be between 0 and 5.");

        return errors;
    }

    public IReadOnlyCollection<string> ValidateRole(string name, IReadOnlyCollection<string> permissions)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(name))
            errors.Add("Role name is required.");

        if (permissions.Any(string.IsNullOrWhiteSpace))
            errors.Add("Permissions cannot be empty.");

        return errors;
    }
}
