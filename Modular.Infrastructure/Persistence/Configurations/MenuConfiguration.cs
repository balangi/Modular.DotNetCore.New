#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modular.Core.AccessControl;

namespace Modular.Infrastructure.Persistence.Configurations;

public sealed class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Route).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Icon).HasMaxLength(128);
        builder.Property(x => x.RequiredPermission).HasMaxLength(256);
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsVisible).IsRequired();
        builder.HasOne(x => x.Module).WithMany(x => x.Menus).HasForeignKey(x => x.ModuleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ModuleId, x.ParentId, x.SortOrder });
        builder.HasIndex(x => x.Route);
    }
}
