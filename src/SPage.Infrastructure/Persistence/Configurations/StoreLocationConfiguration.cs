using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Store;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class StoreLocationConfiguration : IEntityTypeConfiguration<StoreLocation>
{
    public void Configure(EntityTypeBuilder<StoreLocation> builder)
    {
        builder.ToTable("StoreLocations");

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FullAddress).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ProvinceOrCity).HasMaxLength(200);
        builder.Property(x => x.Country).HasMaxLength(200);
        builder.Property(x => x.MapLink).HasMaxLength(500);
        builder.Property(x => x.LandlinePhone).HasMaxLength(50);
        builder.Property(x => x.MobilePhone).HasMaxLength(50);
        builder.Property(x => x.ManagerName).HasMaxLength(200);

        builder.Property(x => x.LocationType).HasConversion<int>();
        builder.Property(x => x.RegionType).HasConversion<int>();

        builder.HasIndex(x => new { x.RegionType, x.LocationType });
    }
}

