using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using SPage.Application.Common.Interfaces;

namespace SPage.Middlewares;

/// <summary>
/// Rewrite URL slug nội bộ (1 query bảng SlugLookups thay vì tra nhiều bảng).
/// Trình duyệt vẫn hiển thị URL gốc, không redirect.
/// </summary>
public sealed class SlugRewriteMiddleware
{
    private static readonly string[] ExcludedPrefixes =
    [
        "/images/", "/css/", "/js/", "/lib/", "/fonts/",
        "/sitemap.xml", "/robots.txt",
        "/api/", "/admin/", "/_",
        "/account/", "/identity/"
    ];

    private readonly RequestDelegate _next;

    public SlugRewriteMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method) ||
            context.Items.ContainsKey(nameof(SlugRewriteMiddleware)))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? "/";
        if (IsExcluded(path) || HasFileExtension(path))
        {
            await _next(context);
            return;
        }

        var slug = ExtractSlug(path);
        if (string.IsNullOrWhiteSpace(slug))
        {
            await _next(context);
            return;
        }

        var db = context.RequestServices.GetRequiredService<IApplicationDbContext>();

        var candidates = await db.SlugLookups
            .AsNoTracking()
            .Where(x => x.Slug == slug)
            .Select(x => new { x.EntityType, x.RewritePath, x.RewriteQuery })
            .ToListAsync(context.RequestAborted);

        var lookup = candidates
            .OrderBy(x => SlugRewritePriority(x.EntityType))
            .Select(x => new { x.RewritePath, x.RewriteQuery })
            .FirstOrDefault();

        if (lookup is not null)
        {
            var merged = new Dictionary<string, StringValues>(
                context.Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase));

            foreach (var part in lookup.RewriteQuery.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = part.IndexOf('=');
                var key = eq >= 0 ? part[..eq].Trim() : part.Trim();
                var value = eq >= 0 && eq < part.Length - 1 ? part[(eq + 1)..].Trim() : string.Empty;
                if (!string.IsNullOrEmpty(key))
                    merged[key] = value;
            }

            context.Request.Path = lookup.RewritePath;
            context.Request.QueryString = QueryString.Create(merged);
            context.Items[nameof(SlugRewriteMiddleware)] = true;
        }

        await _next(context);
    }

    private static bool IsExcluded(string path)
    {
        foreach (var prefix in ExcludedPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static bool HasFileExtension(string path)
    {
        var lastSegment = path.Split('/').LastOrDefault() ?? string.Empty;
        return lastSegment.Contains('.') && lastSegment.IndexOf('.') != 0;
    }

    /// <summary>Thứ tự ưu tiên khi cùng slug (productcategory → category → post → product).</summary>
    private static int SlugRewritePriority(string? entityType)
    {
        return entityType switch
        {
            "productcategory" => 1,
            "category" => 2,
            "post" => 3,
            "product" => 4,
            _ => 5
        };
    }

    private static string ExtractSlug(string path)
    {
        var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return string.Empty;

        if (segments[0].Equals("vi", StringComparison.OrdinalIgnoreCase) ||
            segments[0].Equals("en", StringComparison.OrdinalIgnoreCase))
            return segments.Length >= 2 ? segments[1].ToLowerInvariant() : string.Empty;

        return segments.Length == 1 ? segments[0].ToLowerInvariant() : string.Empty;
    }
}
