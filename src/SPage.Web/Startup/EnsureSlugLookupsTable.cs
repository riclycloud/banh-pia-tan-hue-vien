using Microsoft.EntityFrameworkCore;
using SPage.Infrastructure.Persistence;

namespace SPage.Web.Startup;

/// <summary>
/// Đảm bảo bảng SlugLookups tồn tại (tránh SqlException khi migration chưa chạy đúng DB hoặc bảng bị xóa).
/// </summary>
internal static class EnsureSlugLookupsTable
{
    private const string Sql = """
        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SlugLookups')
        BEGIN
            CREATE TABLE [dbo].[SlugLookups] (
                [Id] int IDENTITY(1,1) NOT NULL,
                [Slug] nvarchar(600) NOT NULL,
                [EntityType] nvarchar(30) NOT NULL,
                [EntityId] int NOT NULL,
                [RewritePath] nvarchar(256) NOT NULL,
                [RewriteQuery] nvarchar(2000) NOT NULL,
                CONSTRAINT [PK_SlugLookups] PRIMARY KEY ([Id])
            );
            CREATE UNIQUE NONCLUSTERED INDEX [IX_SlugLookups_Slug_EntityType] ON [dbo].[SlugLookups]([Slug], [EntityType]);
            CREATE NONCLUSTERED INDEX [IX_SlugLookups_EntityType_EntityId] ON [dbo].[SlugLookups]([EntityType], [EntityId]);
        END
        """;

    public static async Task EnsureSlugLookupsTableAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync(Sql, cancellationToken);
        }
        catch
        {
            // Bảng có thể đã tồn tại do migration; bỏ qua để app vẫn chạy
        }
    }
}
