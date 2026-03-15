using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Contact;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class ContactFieldConfiguration : IEntityTypeConfiguration<ContactField>
{
    public void Configure(EntityTypeBuilder<ContactField> builder)
    {
        builder.ToTable("ContactFields");

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Placeholder)
            .HasMaxLength(300);

        builder.Property(x => x.FieldType)
            .HasConversion<int>();

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}

