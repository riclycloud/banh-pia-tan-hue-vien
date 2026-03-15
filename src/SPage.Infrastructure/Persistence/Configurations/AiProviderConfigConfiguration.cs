using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPage.Domain.Entities.Ai;

namespace SPage.Infrastructure.Persistence.Configurations;

public sealed class AiProviderConfigConfiguration : IEntityTypeConfiguration<AiProviderConfig>
{
    public void Configure(EntityTypeBuilder<AiProviderConfig> builder)
    {
        builder.ToTable("AiProviderConfigs");

        builder.Property(x => x.ProviderKey).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(100);
        builder.Property(x => x.ApiKey).HasMaxLength(500);
        builder.Property(x => x.BaseUrl).HasMaxLength(500);
        builder.Property(x => x.ModelName).HasMaxLength(100);

        builder.HasIndex(x => x.ProviderKey);
    }
}
