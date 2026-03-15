using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Email;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class RecipientEmailConfiguration : IEntityTypeConfiguration<RecipientEmail>
{
    public void Configure(EntityTypeBuilder<RecipientEmail> builder)
    {
        builder.ToTable("RecipientEmails");

        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200);
        builder.Property(x => x.GroupKey).HasMaxLength(50);

        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.GroupKey);
    }
}
