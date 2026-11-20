#nullable enable
using Microsoft.EntityFrameworkCore;
using Modular.Core.AccessControl;
using Modular.Core.Licensing;
using Modular.Core.Modules;

namespace Modular.Infrastructure.Persistence;

public sealed class ModularDbContext(DbContextOptions<ModularDbContext> options) : DbContext(options)
{
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Menu> Menus => Set<Menu>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModularDbContext).Assembly);
    }
}
