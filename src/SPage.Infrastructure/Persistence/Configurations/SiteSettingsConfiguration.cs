using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Seo;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.ToTable("SiteSettings");

        builder.Property(x => x.SiteName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SiteTagline).HasMaxLength(300);
        builder.Property(x => x.DefaultMetaDescription).HasMaxLength(500);

        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.ContactEmail).HasMaxLength(200);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.FacebookPageUrl).HasMaxLength(300);
        builder.Property(x => x.GoogleAnalyticsId).HasMaxLength(50);
        builder.Property(x => x.HomePageId);
    }
}

