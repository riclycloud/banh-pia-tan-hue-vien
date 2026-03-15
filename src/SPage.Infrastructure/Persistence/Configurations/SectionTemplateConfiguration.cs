using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Page;

namespace SPage.Infrastructure.Persistence.Configurations;

internal sealed class SectionTemplateConfiguration : IEntityTypeConfiguration<SectionTemplate>
{
    public void Configure(EntityTypeBuilder<SectionTemplate> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.SectionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.CssClass)
            .HasMaxLength(500);

        // DataJson stored as nvarchar(max) — holds Dictionary<string,string> JSON
        builder.Property(t => t.DataJson)
            .IsRequired()
            .HasDefaultValue("{}");

        builder.HasIndex(t => t.SectionType);
        builder.HasIndex(t => t.IsDeleted).HasFilter("[IsDeleted] = 0");

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
