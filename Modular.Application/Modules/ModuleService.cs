#nullable enable

using Modular.Application.Abstractions;
using Modular.Application.Modules.DTOs;
using Modular.Core.Common;
using Modular.Core.Modules;

namespace Modular.Application.Modules;

public sealed class ModuleService(IModuleRepository repository, ModuleRegistry registry)
{
    private readonly IModuleRepository _repository =
        repository ?? throw new ArgumentNullException(nameof(repository));

    private readonly ModuleRegistry _registry =
        registry ?? throw new ArgumentNullException(nameof(registry));

    public async Task<Result<IReadOnlyList<ModuleDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var reload = await _registry.ReloadAsync(cancellationToken).ConfigureAwait(false);

        if (reload.IsFailure)
        {
            return Result<IReadOnlyList<ModuleDto>>.Failure(
                reload.ErrorCode ?? "module.registry_reload_failed",
                reload.ErrorMessage ?? "Failed to reload the module registry.");
        }

        var modules = reload.Value?
            .Select(ToDto)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        return Result<IReadOnlyList<ModuleDto>>.Success(modules);
    }

    public async Task<Result<ModuleDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Result<ModuleDto>.Failure(
                "module.invalid_id",
                "Module identifier is required.");
        }

        var module = await _repository
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (module is null)
        {
            return Result<ModuleDto>.Failure(
                "module.not_found",
                $"Module '{id}' was not found.");
        }

        return Result<ModuleDto>.Success(ToDto(module));
    }

    public async Task<Result<ModuleDto>> UpdateAsync(
        Guid id,
        UpdateModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Result<ModuleDto>.Failure(
                "module.invalid_id",
                "Module identifier is required.");
        }

        ArgumentNullException.ThrowIfNull(request);

        var current = await _repository
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (current is null)
        {
            return Result<ModuleDto>.Failure(
                "module.not_found",
                $"Module '{id}' was not found.");
        }

        var name = request.Name?.Trim() ?? current.Name;

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ModuleDto>.Failure(
                "module.invalid_name",
                "Module name cannot be empty.");
        }

        // هر دو سمت انتخاب به IEnumerable<string> تبدیل می‌شوند تا
        // ناسازگاری IReadOnlyList و ICollection رخ ندهد.
        IEnumerable<string> dependencies =
            request.Dependencies is not null
                ? request.Dependencies
                : current.Dependencies;

        IEnumerable<string> allowedRoles =
            request.AllowedRoles is not null
                ? request.AllowedRoles
                : current.AllowedRoles;

        IEnumerable<string> requiredPermissions =
            request.RequiredPermissions is not null
                ? request.RequiredPermissions
                : current.RequiredPermissions;

        var normalizedDependencies = Normalize(dependencies);

        if (normalizedDependencies.Any(dependency =>
                StringComparer.OrdinalIgnoreCase.Equals(dependency, current.Key)))
        {
            return Result<ModuleDto>.Failure(
                "module.self_dependency",
                "A module cannot depend on itself.");
        }

        var description = request.Description is null
            ? current.Description
            : NullIfWhiteSpace(request.Description);

        var productCode = request.RequiredProductCode is null
            ? current.RequiredProductCode
            : NullIfWhiteSpace(request.RequiredProductCode);

        current.Update(
            name,
            description,
            request.Status ?? current.Status,
            normalizedDependencies,
            productCode,
            request.MinimumAccessLevel ?? current.MinimumAccessLevel,
            Normalize(allowedRoles),
            Normalize(requiredPermissions));

        _repository.Update(current);

        await _repository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        var reload = await _registry
            .ReloadAsync(cancellationToken)
            .ConfigureAwait(false);

        if (reload.IsFailure)
        {
            return Result<ModuleDto>.Failure(
                reload.ErrorCode ?? "module.registry_reload_failed",
                reload.ErrorMessage ?? "Failed to reload the module registry.");
        }

        return Result<ModuleDto>.Success(ToDto(current));
    }

    private static ModuleDto ToDto(Module module)
    {
        return new ModuleDto(
            module.Id.ToString(),
            module.Name,
            module.Description ?? string.Empty,
            module.Status,
            Normalize(module.Dependencies),
            module.RequiredProductCode,
            module.MinimumAccessLevel,
            Normalize(module.AllowedRoles),
            Normalize(module.RequiredPermissions));
    }

    private static string[] Normalize(IEnumerable<string>? values)
    {
        return (values ?? Enumerable.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
