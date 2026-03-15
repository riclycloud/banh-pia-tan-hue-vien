using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Infrastructure.Persistence;

namespace SPage.Web.SeedData;

internal static partial class DataSeeder
{
    /// <summary>
    /// Đồng bộ bảng SlugLookups từ toàn bộ Post, Category, ProductCategory, Product (active).
    /// Chạy sau khi có dữ liệu; idempotent — gọi mỗi lần khởi động app cũng an toàn.
    /// </summary>
    public static async Task SeedSlugLookupsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var slugLookup = scope.ServiceProvider.GetRequiredService<ISlugLookupService>();

        var ct = CancellationToken.None;

        foreach (var post in await db.Posts.AsNoTracking().Select(p => new { p.Id, p.Slug }).ToListAsync(ct))
            await slugLookup.SyncPostAsync(post.Slug, post.Id, ct);

        foreach (var cat in await db.Categories.AsNoTracking().Select(c => new { c.Id, c.Slug }).ToListAsync(ct))
            await slugLookup.SyncCategoryAsync(cat.Slug, cat.Id, ct);

        foreach (var pc in await db.ProductCategories.AsNoTracking().Select(pc => new { pc.Id, pc.Slug }).ToListAsync(ct))
            await slugLookup.SyncProductCategoryAsync(pc.Slug, pc.Id, ct);

        var products = await db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.Slug, CategorySlug = p.Category != null ? p.Category.Slug : "san-pham" })
            .ToListAsync(ct);

        foreach (var p in products)
            await slugLookup.SyncProductAsync(p.Slug, p.Id, p.CategorySlug, ct);

        await db.SaveChangesAsync(ct);
    }
}
