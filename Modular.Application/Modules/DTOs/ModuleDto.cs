#nullable enable
using Modular.Core.AccessControl;
using Modular.Core.Enums;

namespace Modular.Application.Modules.DTOs;

/// <summary>A read-only representation of a registered module.</summary>
public sealed record ModuleDto(string Id, string Name, string Description, ModuleStatus Status,
    IReadOnlyList<string> Dependencies, string? RequiredProductCode, AccessLevel MinimumAccessLevel,
    IReadOnlyList<string> AllowedRoles, IReadOnlyList<string> RequiredPermissions);
