using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Seo;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class SlugLookupConfiguration : IEntityTypeConfiguration<SlugLookup>
{
    public void Configure(EntityTypeBuilder<SlugLookup> builder)
    {
        builder.ToTable("SlugLookups");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).HasMaxLength(600).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.RewritePath).HasMaxLength(256).IsRequired();
        builder.Property(x => x.RewriteQuery).HasMaxLength(2000).IsRequired();

        builder.HasIndex(x => new { x.Slug, x.EntityType }).IsUnique().HasDatabaseName("IX_SlugLookups_Slug_EntityType");
        builder.HasIndex(x => new { x.EntityType, x.EntityId }).HasDatabaseName("IX_SlugLookups_EntityType_EntityId");
    }
}
