using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Contact;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");

        builder.Property(x => x.Subject).HasMaxLength(300);
        builder.Property(x => x.SenderName).HasMaxLength(200);
        builder.Property(x => x.SenderEmail).HasMaxLength(200);
        builder.Property(x => x.SenderPhone).HasMaxLength(50);
        builder.Property(x => x.SenderIp).HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.DataJson)
            .IsRequired();
    }
}

