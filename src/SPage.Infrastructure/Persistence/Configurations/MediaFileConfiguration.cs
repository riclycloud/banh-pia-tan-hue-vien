using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Media;

namespace SPage.Infrastructure.Persistence.Configurations;

internal sealed class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.StoredName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.MimeType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.AltText)
            .HasMaxLength(500);

        builder.HasIndex(f => f.CreatedAt);
        builder.HasIndex(f => f.IsDeleted).HasFilter("[IsDeleted] = 0");

        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}
