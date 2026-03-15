using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Navigation;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class MenuLocationConfiguration : IEntityTypeConfiguration<MenuLocation>
{
    public void Configure(EntityTypeBuilder<MenuLocation> builder)
    {
        builder.ToTable("MenuLocations");
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => x.Key).IsUnique().HasDatabaseName("IX_MenuLocations_Key");
        builder.HasIndex(x => x.SortOrder).HasDatabaseName("IX_MenuLocations_SortOrder");
    }
}

public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(500).IsRequired();

        builder.HasOne(x => x.Location)
            .WithMany(x => x.MenuItems)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.LocationId, x.SortOrder }).HasDatabaseName("IX_MenuItems_Location_Sort");
    }
}
