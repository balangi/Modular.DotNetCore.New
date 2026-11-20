#nullable enable
using Modular.Application.Abstractions;
using Modular.Application.Navigation.DTOs;
using Modular.Core.Common;

namespace Modular.Application.Navigation;

/// <summary>Retrieves and persists admin-managed panel layouts.</summary>
public sealed class PanelLayoutService(IModuleRepository modules)
{
    public async Task<PanelLayoutDto> GetAsync(string panelKey, CancellationToken ct = default)
    {
        var layout = (await modules.GetPanelLayoutsAsync(panelKey, ct).ConfigureAwait(false)).OrderByDescending(x => x.Version).FirstOrDefault();
        return layout is null ? new PanelLayoutDto(panelKey, $"{"items":[]}", 0, true) : new PanelLayoutDto(layout.PanelKey, layout.LayoutJson, layout.Version, false);
    }

    public async Task<Result<PanelLayoutDto>> SaveAsync(PanelLayoutDefinition layout, string actorUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(layout.PanelKey) || string.IsNullOrWhiteSpace(layout.LayoutJson)) 
            return Result<PanelLayoutDto>.Failure("invalid_layout", "Panel key and layout JSON are required.");

        var candidate = layout with { UpdatedAt = DateTimeOffset.UtcNow, UpdatedBy = actorUserId };
        var result = await modules.UpsertPanelLayoutAsync(candidate, ct).ConfigureAwait(false);
        
        return result.IsFailure ? Result<PanelLayoutDto>.Failure(result.ErrorCode ?? "save_failed", result.ErrorMessage ?? "Layout could not be saved.") : Result<PanelLayoutDto>.Success(new PanelLayoutDto(candidate.PanelKey, candidate.LayoutJson, candidate.Version, false));
    }
}
