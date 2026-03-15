using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Video;

namespace SPage.Infrastructure.Persistence.Configurations;

internal sealed class VideoCategoryConfiguration : IEntityTypeConfiguration<VideoCategory>
{
    public void Configure(EntityTypeBuilder<VideoCategory> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasIndex(c => c.DisplayOrder);
        builder.HasIndex(c => c.IsDeleted).HasFilter("[IsDeleted] = 0");

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
