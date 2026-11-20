using Modular.Core.Enums;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Modular.Core.Modules;

/// <summary>Immutable metadata and access policy for a module.</summary>
public sealed class ModuleDescriptor
{
    private static readonly Regex ProductCodePattern = new("^[A-Za-z0-9][A-Za-z0-9._-]{0,63}$", RegexOptions.CultureInvariant);

    /// <summary>Stable code for unauthenticated public access.</summary>
    public const string PublicAccess = "Public";

    /// <summary>Stable code for an authenticated ordinary user.</summary>
    public const string UserAccess = "User";

    /// <summary>Stable code for an administrator.</summary>
    public const string AdministratorAccess = "Administrator";

    /// <summary>Creates a validated module descriptor.</summary>
    public ModuleDescriptor(
        string id,
        string name,
        Version version,
        ModuleType type = ModuleType.General,
        string? productCode = null,
        IEnumerable<string>? accessLevels = null,
        IEnumerable<ModuleDependency>? dependencies = null,
        ModuleStatus status = ModuleStatus.Discovered)
    {
        Id = ValidateId(id, nameof(id));
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("The module name is required.", nameof(name)) : name.Trim();
        if (Name.Length > 200) throw new ArgumentException("The module name cannot exceed 200 characters.", nameof(name));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        if (Version.Major < 0 || Version.Minor < 0) throw new ArgumentOutOfRangeException(nameof(version), "Version components cannot be negative.");
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        Type = type;
        ProductCode = type == ModuleType.ProductSpecific
            ? ValidateProductCode(productCode, nameof(productCode))
            : ValidateOptionalProductCode(productCode, nameof(productCode));
        AccessLevels = new ReadOnlyCollection<string>(NormalizeAccessLevels(accessLevels));
        Dependencies = new ReadOnlyCollection<ModuleDependency>(ValidateDependencies(dependencies));
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status));
        Status = status;
    }

    /// <summary>Gets the stable module identifier.</summary>
    public string Id { get; }
    /// <summary>Gets the display name.</summary>
    public string Name { get; }
    /// <summary>Gets the semantic module version.</summary>
    public Version Version { get; }
    /// <summary>Gets the module applicability type.</summary>
    public ModuleType Type { get; }
    /// <summary>Gets the product code used for product-specific licensing, or null.</summary>
    public string? ProductCode { get; }
    /// <summary>Gets stable access-level codes accepted by the module. Public means no login is required.</summary>
    public IReadOnlyList<string> AccessLevels { get; }
    /// <summary>Gets the module dependency declarations.</summary>
    public IReadOnlyList<ModuleDependency> Dependencies { get; }
    /// <summary>Gets the current lifecycle/deployment state.</summary>
    public ModuleStatus Status { get; }

    private static string ValidateId(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("The module ID is required.", parameterName);
        if (value.Length > 100) throw new ArgumentException("The module ID cannot exceed 100 characters.", parameterName);
        foreach (var c in value)
            if (!(char.IsLetterOrDigit(c) || c is '.' or '-' or '_')) throw new ArgumentException("The module ID may contain letters, digits, '.', '-' and '_'.", parameterName);
        return value;
    }

    private static string ValidateProductCode(string? value, string parameterName) =>
        string.IsNullOrWhiteSpace(value) || !ProductCodePattern.IsMatch(value)
            ? throw new ArgumentException("A product-specific module requires a product code using 1-64 letters, digits, '.', '-' or '_'.", parameterName)
            : value;

    private static string? ValidateOptionalProductCode(string? value, string parameterName) =>
        value is null ? null : ValidateProductCode(value, parameterName);

    private static List<string> NormalizeAccessLevels(IEnumerable<string>? values)
    {
        var result = (values ?? new[] { PublicAccess }).Select(value =>
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Access levels cannot be empty.", nameof(values));
            var normalized = value.Trim();
            if (normalized.Length > 64 || normalized.Any(char.IsControl)) throw new ArgumentException("Access levels must be printable codes of at most 64 characters.", nameof(values));
            return normalized;
        }).Distinct(StringComparer.Ordinal).ToList();
        if (result.Count == 0) throw new ArgumentException("At least one access level is required.", nameof(values));
        return result;
    }

    private List<ModuleDependency> ValidateDependencies(IEnumerable<ModuleDependency>? values)
    {
        var result = (values ?? Array.Empty<ModuleDependency>()).ToList();
        if (result.Any(dependency => dependency is null)) throw new ArgumentException("Dependencies cannot contain null.", nameof(values));
        if (result.Any(dependency => string.Equals(dependency.ModuleId, Id, StringComparison.Ordinal))) throw new ArgumentException("A module cannot depend on itself.", nameof(values));
        if (result.GroupBy(dependency => dependency.ModuleId, StringComparer.Ordinal).Any(group => group.Count() > 1)) throw new ArgumentException("Dependencies cannot repeat a module ID.", nameof(values));
        return result;
    }
}
