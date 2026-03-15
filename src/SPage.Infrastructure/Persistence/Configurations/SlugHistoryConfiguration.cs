using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Seo;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class SlugHistoryConfiguration : IEntityTypeConfiguration<SlugHistory>
{
    public void Configure(EntityTypeBuilder<SlugHistory> builder)
    {
        builder.ToTable("SlugHistories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OldSlug).HasMaxLength(700).IsRequired();
        builder.Property(x => x.NewSlug).HasMaxLength(700).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.RedirectPath).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.ChangedBy).HasMaxLength(256);

        // ★ Index quan trọng nhất — middleware tra cứu OldSlug mỗi request 404
        builder.HasIndex(x => x.OldSlug)
            .HasDatabaseName("IX_SlugHistories_OldSlug");

        // Composite index để tra cứu toàn bộ lịch sử của 1 entity
        builder.HasIndex(x => new { x.EntityType, x.EntityId })
            .HasDatabaseName("IX_SlugHistories_EntityType_EntityId");
    }
}
