using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Blog;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(250).IsRequired();
        builder.Property(x => x.MetaTitle).HasMaxLength(70);
        builder.Property(x => x.MetaDescription).HasMaxLength(160);

        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_Categories_Slug");
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
