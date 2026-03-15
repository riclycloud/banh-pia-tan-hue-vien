using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Enums;
using SPage.Infrastructure.Persistence;

namespace SPage.Infrastructure.Services;

/// <summary>
/// Sitemap XML động theo chuẩn sitemaps.org — cập nhật từ DB, tối ưu SEO (lastmod ISO8601, priority, changefreq).
/// </summary>
public sealed class SitemapService : ISitemapService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public SitemapService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var baseUrl = GetBaseUrl();
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urls = new List<XElement>();

        // Homepage — ưu tiên cao, cập nhật hàng ngày
        urls.Add(CreateUrl(ns, baseUrl, "/", "1.0", "daily", null));

        // Dynamic Pages (trang tĩnh: giới thiệu, liên hệ...)
        var pages = await _context.DynamicPages
            .AsNoTracking()
            .Where(p => p.Status == PageStatus.Published && p.IsIndexable)
            .Select(p => new { p.Slug, p.LastModifiedAt })
            .ToListAsync(cancellationToken);
        foreach (var p in pages)
            urls.Add(CreateUrl(ns, baseUrl, $"/{p.Slug}", "0.8", "monthly", p.LastModifiedAt));

        // Danh mục bài viết (Category)
        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsIndexable)
            .Select(c => new { c.Slug, c.LastModifiedAt })
            .ToListAsync(cancellationToken);
        foreach (var c in categories)
            urls.Add(CreateUrl(ns, baseUrl, $"/{c.Slug}", "0.7", "weekly", c.LastModifiedAt));

        // Bài viết (Post)
        var posts = await _context.Posts
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Published && p.IsIndexable)
            .Select(p => new { p.Slug, p.LastModifiedAt, p.PublishedAt })
            .ToListAsync(cancellationToken);
        foreach (var p in posts)
            urls.Add(CreateUrl(ns, baseUrl, $"/{p.Slug}", "0.8", "weekly", p.LastModifiedAt ?? p.PublishedAt));

        // Danh mục sản phẩm (ProductCategory)
        var productCategories = await _context.ProductCategories
            .AsNoTracking()
            .Where(pc => pc.IsIndexable)
            .Select(pc => new { pc.Slug, pc.LastModifiedAt })
            .ToListAsync(cancellationToken);
        foreach (var pc in productCategories)
            urls.Add(CreateUrl(ns, baseUrl, $"/{pc.Slug}", "0.7", "weekly", pc.LastModifiedAt));

        // Sản phẩm (Product)
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.IsIndexable)
            .Select(p => new { p.Slug, p.LastModifiedAt })
            .ToListAsync(cancellationToken);
        foreach (var p in products)
            urls.Add(CreateUrl(ns, baseUrl, $"/{p.Slug}", "0.7", "weekly", p.LastModifiedAt));

        var sitemap = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(ns + "urlset", urls));

        return sitemap.ToString();
    }

    private string GetBaseUrl()
    {
        var configured = _configuration["Seo:BaseUrl"]?.Trim();
        if (!string.IsNullOrEmpty(configured))
        {
            if (!configured.EndsWith("/", StringComparison.Ordinal))
                configured += "/";
            return configured.TrimEnd('/');
        }
        var request = _httpContextAccessor.HttpContext?.Request;
        return request is not null
            ? $"{request.Scheme}://{request.Host}"
            : "https://localhost";
    }

    /// <summary>Chuẩn SEO: lastmod ISO 8601 (Google khuyến nghị), loc tuyệt đối, changefreq + priority.</summary>
    private static XElement CreateUrl(
        XNamespace ns,
        string baseUrl,
        string path,
        string priority,
        string changefreq,
        DateTimeOffset? lastmod = null)
    {
        var loc = path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : $"{baseUrl}{path}";
        var elements = new List<object>
        {
            new XElement(ns + "loc", loc),
            new XElement(ns + "changefreq", changefreq),
            new XElement(ns + "priority", priority)
        };
        if (lastmod.HasValue)
            elements.Add(new XElement(ns + "lastmod", lastmod.Value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ")));
        return new XElement(ns + "url", elements);
    }
}
