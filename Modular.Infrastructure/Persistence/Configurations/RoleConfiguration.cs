#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modular.Core.AccessControl;

namespace Modular.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.AccessLevel).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.NormalizedName).IsUnique();
        builder.HasIndex(x => x.AccessLevel);
    }
}
