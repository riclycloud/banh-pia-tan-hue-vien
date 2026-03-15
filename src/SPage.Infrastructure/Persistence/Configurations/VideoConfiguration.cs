using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Video;

namespace SPage.Infrastructure.Persistence.Configurations;

internal sealed class VideoConfiguration : IEntityTypeConfiguration<VideoEntity>
{
    public void Configure(EntityTypeBuilder<VideoEntity> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(v => v.Slug)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(v => v.VideoId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.VideoUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(v => v.ThumbnailUrl)
            .HasMaxLength(1000);

        builder.Property(v => v.Duration)
            .HasMaxLength(50);

        builder.Property(v => v.Description)
            .HasMaxLength(2000);

        builder.Property(v => v.TagsCsv)
            .HasMaxLength(500);

        builder.Property(v => v.MetaTitle)
            .HasMaxLength(200);

        builder.Property(v => v.MetaDescription)
            .HasMaxLength(500);

        builder.Property(v => v.CanonicalUrl)
            .HasMaxLength(500);

        builder.Property(v => v.Platform)
            .HasConversion<int>();

        builder.HasIndex(v => v.Slug).IsUnique();
        builder.HasIndex(v => v.Platform);
        builder.HasIndex(v => v.IsPublished);
        builder.HasIndex(v => v.IsFeatured);
        builder.HasIndex(v => v.IsDeleted).HasFilter("[IsDeleted] = 0");

        builder.HasOne(v => v.Category)
            .WithMany(c => c.Videos)
            .HasForeignKey(v => v.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
