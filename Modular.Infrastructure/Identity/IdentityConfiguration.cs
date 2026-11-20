using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Modular.Infrastructure.Runtime.Identity;

/// <summary>EF Core mappings. Apply these from the host's IdentityDbContext model builder.</summary>
public static class IdentityConfiguration
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
    }

    private sealed class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> b)
        {
            b.ToTable("AspNetUsers");
            b.Property(x => x.DisplayName).HasMaxLength(200);
            b.Property(x => x.LoginLevel).HasMaxLength(64);
            b.Property(x => x.CustomData).HasColumnType("nvarchar(max)");
            b.HasIndex(x => new { x.TenantId, x.NormalizedUserName }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.NormalizedEmail });
            b.HasQueryFilter(x => x.IsActive);
        }
    }

    private sealed class RoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> b)
        {
            b.ToTable("AspNetRoles");
            b.Property(x => x.DisplayName).HasMaxLength(200);
            b.Property(x => x.CustomData).HasColumnType("nvarchar(max)");
            b.HasIndex(x => new { x.TenantId, x.NormalizedName }).IsUnique();
        }
    }
}
