using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Product;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(250).IsRequired();
        builder.Property(x => x.MetaTitle).HasMaxLength(70);
        builder.Property(x => x.MetaDescription).HasMaxLength(160);
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(500);
        builder.Property(x => x.ThumbnailAlt).HasMaxLength(200);

        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_ProductCategories_Slug");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
