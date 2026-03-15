using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Product;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.Property(x => x.Name).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(600).IsRequired();
        builder.Property(x => x.MetaTitle).HasMaxLength(70);
        builder.Property(x => x.MetaDescription).HasMaxLength(160);
        builder.Property(x => x.SKU).HasMaxLength(100);
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SalePrice).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_Products_Slug");
        builder.HasIndex(x => x.CategoryId).HasDatabaseName("IX_Products_CategoryId");
        builder.HasIndex(x => new { x.IsActive, x.SortOrder }).HasDatabaseName("IX_Products_Active_Sort");
        builder.HasIndex(x => x.IsNew).HasDatabaseName("IX_Products_IsNew");
        builder.HasIndex(x => x.IsFeatured).HasDatabaseName("IX_Products_IsFeatured");
        builder.HasIndex(x => x.IsPromotion).HasDatabaseName("IX_Products_IsPromotion");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.AltText).HasMaxLength(200);
        builder.HasIndex(x => new { x.ProductId, x.IsPrimary }).HasDatabaseName("IX_ProductImages_Primary");
    }
}
