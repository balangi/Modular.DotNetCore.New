#nullable enable

using System;
using Modular.Core.Common;

namespace Modular.Core.AccessControl;

public sealed class AccessLevel : Entity
{
    public static readonly Guid AnonymousId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid PublicId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid UserId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid ManagerId = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid AdminId = Guid.Parse("00000000-0000-0000-0000-000000000005");
    public static readonly Guid SuperAdminId = Guid.Parse("00000000-0000-0000-0000-000000000006");

    public AccessLevel(Guid id, string key, string name, int rank = 0, bool isSystemDefined = false) : base(id)
    {
        Key = Validate(key, nameof(key), 100);
        Name = Validate(name, nameof(name), 200);
        if (rank < 0) throw new ArgumentOutOfRangeException(nameof(rank));
        Rank = rank;
        IsSystemDefined = isSystemDefined;
    }

    public string Key { get; private set; }
    public string Name { get; private set; }
    public int Rank { get; private set; }
    public bool IsSystemDefined { get; }

    public void Rename(string name) { Name = Validate(name, nameof(name), 200); Touch(); }
    public void ChangeRank(int rank) { if (rank < 0) throw new ArgumentOutOfRangeException(nameof(rank)); Rank = rank; Touch(); }

    public static AccessLevel Anonymous => new(AnonymousId, "anonymous", "Anonymous", -10, true);
    public static AccessLevel Public => new(PublicId, "public", "Public", 0, true);
    public static AccessLevel User => new(UserId, "user", "User", 10, true);
    public static AccessLevel Manager => new(ManagerId, "manager", "Manager", 50, true);
    public static AccessLevel Admin => new(AdminId, "admin", "Administrator", 80, true);
    public static AccessLevel SuperAdmin => new(SuperAdminId, "super-admin", "Super Administrator", 100, true);

    private static string Validate(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength) throw new ArgumentOutOfRangeException(parameterName);
        return normalized;
    }
}
