using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Content;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class SliderConfiguration : IEntityTypeConfiguration<Slider>, IEntityTypeConfiguration<Slide>
{
    public void Configure(EntityTypeBuilder<Slider> builder)
    {
        builder.ToTable("Sliders");
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Position).HasConversion<int>();
        builder.HasMany(x => x.Slides)
            .WithOne(x => x.Slider)
            .HasForeignKey(x => x.SliderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public void Configure(EntityTypeBuilder<Slide> builder)
    {
        builder.ToTable("Slides");
        builder.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.AltText).HasMaxLength(300);
        builder.Property(x => x.Caption).HasMaxLength(500);
        builder.Property(x => x.LinkUrl).HasMaxLength(500);

        builder.HasIndex(x => new { x.SliderId, x.SortOrder });
    }
}

