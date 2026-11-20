#nullable enable
using Modular.Core.AccessControl;
using Modular.Core.Enums;

namespace Modular.Application.Modules.DTOs;

/// <summary>Partial module update. Null fields are left unchanged.</summary>
public sealed record UpdateModuleRequest(string? Name = null, string? Description = null,
    ModuleStatus? Status = null, IReadOnlyList<string>? Dependencies = null,
    string? RequiredProductCode = null, AccessLevel? MinimumAccessLevel = null,
    IReadOnlyList<string>? AllowedRoles = null, IReadOnlyList<string>? RequiredPermissions = null);
