using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Blog;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_Tags_Slug");
    }
}

public sealed class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
{
    public void Configure(EntityTypeBuilder<PostTag> builder)
    {
        builder.ToTable("PostTags");
        builder.HasKey(x => new { x.PostId, x.TagId });
        builder.HasOne(x => x.Post).WithMany(p => p.PostTags).HasForeignKey(x => x.PostId);
        builder.HasOne(x => x.Tag).WithMany(t => t.PostTags).HasForeignKey(x => x.TagId);
    }
}
