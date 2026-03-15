using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Page;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class DynamicPageConfiguration : IEntityTypeConfiguration<DynamicPage>
{
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public void Configure(EntityTypeBuilder<DynamicPage> builder)
    {
        builder.ToTable("DynamicPages");
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(600).IsRequired();
        builder.Property(x => x.MetaTitle).HasMaxLength(70);
        builder.Property(x => x.MetaDescription).HasMaxLength(160);
        builder.Property(x => x.Template).HasMaxLength(100).HasDefaultValue("Default");

        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_DynamicPages_Slug");
        builder.HasQueryFilter(x => !x.IsDeleted);

        // ★ JSON Column — EF Core 8 — lưu toàn bộ List<PageSectionData> thành 1 cột JSON
        builder.OwnsMany(x => x.Sections, sections =>
        {
            sections.ToJson("Sections");
            sections.Property(s => s.SectionType).HasMaxLength(50);
            sections.Property(s => s.CssClass).HasMaxLength(200);

            // Dictionary<string,string> không được EF Core 8 hỗ trợ natively trong JSON column
            // → dùng value converter: serialize/deserialize thành JSON string
            sections.Property(s => s.Data)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, _jsonOpts),
                    v => string.IsNullOrEmpty(v)
                        ? new Dictionary<string, string>()
                        : JsonSerializer.Deserialize<Dictionary<string, string>>(v, _jsonOpts)
                          ?? new Dictionary<string, string>());
        });
    }
}
