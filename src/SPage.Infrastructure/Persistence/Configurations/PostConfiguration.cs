using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Blog;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(600).IsRequired();
        builder.Property(x => x.MetaTitle).HasMaxLength(70);
        builder.Property(x => x.MetaDescription).HasMaxLength(160);
        builder.Property(x => x.CanonicalUrl).HasMaxLength(2048);
        builder.Property(x => x.SchemaType).HasMaxLength(50).HasDefaultValue("Article");
        builder.Property(x => x.Content).HasColumnType("nvarchar(max)");

        // ★ Optimized Indexes
        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_Posts_Slug");
        builder.HasIndex(x => x.CategoryId).HasDatabaseName("IX_Posts_CategoryId");
        builder.HasIndex(x => new { x.Status, x.PublishedAt }).HasDatabaseName("IX_Posts_Status_PublishedAt");
        builder.HasIndex(x => x.IsDeleted).HasFilter("[IsDeleted] = 0").HasDatabaseName("IX_Posts_Active");

        // Soft delete global filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Category)
            .WithMany(c => c.Posts)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
