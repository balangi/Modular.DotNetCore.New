#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modular.Core.Licensing;

namespace Modular.Infrastructure.Persistence.Configurations;

public sealed class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.LicenseKey).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ProductType).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(x => x.Signature).HasMaxLength(4096);
        builder.Property(x => x.IssuedDate).IsRequired();
        builder.Property(x => x.ExpiryDate);
        builder.HasIndex(x => x.LicenseKey).IsUnique();
        builder.HasIndex(x => new { x.Status, x.ExpiryDate });
    }
}
