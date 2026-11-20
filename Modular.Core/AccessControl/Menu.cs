#nullable enable
using Modular.Core.Modules;

namespace Modular.Core.AccessControl;

public sealed class Menu
{
    private Menu() { }
    public Guid Id { get; private set; }
    public Guid ModuleId { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Route { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public string? RequiredPermission { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsVisible { get; private set; }
    public Module Module { get; private set; } = null!;
    public Menu? Parent { get; private set; }
    public ICollection<Menu> Children { get; private set; } = new List<Menu>();
}
