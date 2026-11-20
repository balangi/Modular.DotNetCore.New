namespace Modular.Core.Modules;

/// <summary>Describes another module required or optionally used by a module.</summary>
public sealed class ModuleDependency : IEquatable<ModuleDependency>
{
    /// <summary>Creates a dependency declaration.</summary>
    /// <param name="moduleId">Stable identifier of the depended-on module.</param>
    /// <param name="minimumVersion">Minimum compatible version.</param>
    /// <param name="required">Whether startup must fail if this dependency is unavailable.</param>
    public ModuleDependency(string moduleId, Version minimumVersion, bool required = true)
    {
        ModuleId = ValidateId(moduleId, nameof(moduleId));
        MinimumVersion = minimumVersion ?? throw new ArgumentNullException(nameof(minimumVersion));
        if (minimumVersion.Major < 0 || minimumVersion.Minor < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumVersion), "Version components cannot be negative.");
        Required = required;
    }

    /// <summary>Gets the stable identifier of the depended-on module.</summary>
    public string ModuleId { get; }

    /// <summary>Gets the minimum version accepted by this dependency.</summary>
    public Version MinimumVersion { get; }

    /// <summary>Gets a value indicating whether the dependency is required.</summary>
    public bool Required { get; }

    /// <inheritdoc />
    public bool Equals(ModuleDependency? other) => other is not null &&
        string.Equals(ModuleId, other.ModuleId, StringComparison.Ordinal) &&
        Equals(MinimumVersion, other.MinimumVersion) && Required == other.Required;

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ModuleDependency);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(ModuleId, MinimumVersion, Required);

    private static string ValidateId(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("The module ID is required.", parameterName);
        if (value.Length > 100)
            throw new ArgumentException("The module ID cannot exceed 100 characters.", parameterName);
        foreach (var character in value)
            if (!(char.IsLetterOrDigit(character) || character is '.' or '-' or '_'))
                throw new ArgumentException("The module ID may contain letters, digits, '.', '-' and '_'.", parameterName);
        return value;
    }
}
