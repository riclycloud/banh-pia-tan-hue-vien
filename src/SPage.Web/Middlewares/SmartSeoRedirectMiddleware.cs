using SPage.Application.Common.Interfaces;

namespace SPage.Middlewares;

/// <summary>
/// Smart 301 SEO Redirect Middleware.
///
/// Luồng hoạt động:
///  1. Trước khi chạy pipeline chính: chuẩn hóa URL (trailing slash, uppercase → lowercase).
///  2. Sau khi pipeline chạy: nếu response là 404, kiểm tra SlugHistory trong DB.
///     Nếu tìm thấy slug cũ → trả về 301 Moved Permanently tới URL mới.
///     Google Bot và trình duyệt sẽ cập nhật bookmark/index tự động.
///
/// Cache Strategy:
///  - Lần đầu: query DB, lưu vào IMemoryCache (TTL 6h).
///  - Lần sau: trả từ cache trong microseconds, KHÔNG tốn DB round-trip.
///  - Slug không tồn tại: cache sentinel "__null__" để tránh DB spam.
/// </summary>
public sealed class SmartSeoRedirectMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SmartSeoRedirectMiddleware> _logger;

    // Prefix paths không cần kiểm tra slug history
    private static readonly string[] ExcludedPrefixes =
    [
        "/images/", "/css/", "/js/", "/lib/", "/fonts/",
        "/sitemap.xml", "/robots.txt",
        "/api/", "/admin/", "/_",
        "/account/", "/identity/"
    ];

    public SmartSeoRedirectMiddleware(
        RequestDelegate next,
        ILogger<SmartSeoRedirectMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";

        // ── Pass 1: URL normalization (trước khi chạy pipeline) ───────
        if (!IsExcluded(path))
        {
            // Redirect trailing slash (trừ root)
            if (path.Length > 1 && path.EndsWith('/'))
            {
                Redirect301(context, path.TrimEnd('/') + context.Request.QueryString);
                return;
            }

            // Redirect uppercase → lowercase
            var lower = path.ToLowerInvariant();
            if (path != lower)
            {
                Redirect301(context, lower + context.Request.QueryString);
                return;
            }
        }

        // ── Chạy pipeline chính ───────────────────────────────────────
        await _next(context);

        // ── Pass 2: Slug history check sau khi nhận 404 ───────────────
        if (context.Response.StatusCode != 404
            || context.Response.HasStarted
            || IsExcluded(path))
        {
            return;
        }

        // Trích xuất slug từ path
        // Hỗ trợ: /blog/old-slug, /san-pham/old-slug, /old-slug
        var slug = ExtractSlug(path);
        if (string.IsNullOrEmpty(slug)) return;

        // Resolve từ DI (Scoped service — phải resolve qua IServiceProvider)
        var slugHistoryService = context.RequestServices
            .GetRequiredService<ISlugHistoryService>();

        var redirectPath = await slugHistoryService.GetRedirectPathAsync(slug, context.RequestAborted);

        if (redirectPath is not null)
        {
            _logger.LogInformation(
                "SEO 301: '{OldPath}' → '{NewPath}' (slug history match)",
                path, redirectPath);

            // Reset response 404 → 301
            context.Response.Clear();
            Redirect301(context, redirectPath);
        }
    }

    // Trích xuất slug từ nhiều dạng URL
    private static string ExtractSlug(string path)
    {
        var segments = path.Trim('/').Split('/');
        return segments.Length switch
        {
            // /old-slug → "old-slug"
            1 => segments[0],
            // /blog/old-slug hoặc /san-pham/old-slug → "old-slug"
            2 => segments[1],
            _ => string.Empty
        };
    }

    private static void Redirect301(HttpContext context, string location)
    {
        context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
        context.Response.Headers.Location = location.ToString();
    }

    private static bool IsExcluded(string path)
    {
        foreach (var prefix in ExcludedPrefixes)
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }
}
