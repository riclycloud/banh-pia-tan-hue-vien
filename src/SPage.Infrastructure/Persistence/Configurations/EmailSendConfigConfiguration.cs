using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Email;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class EmailSendConfigConfiguration : IEntityTypeConfiguration<EmailSendConfig>
{
    public void Configure(EntityTypeBuilder<EmailSendConfig> builder)
    {
        builder.ToTable("EmailSendConfigs");

        builder.Property(x => x.Host).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FromEmail).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FromName).HasMaxLength(200);
        builder.Property(x => x.UserName).HasMaxLength(200);
        builder.Property(x => x.Password).HasMaxLength(500);
    }
}
