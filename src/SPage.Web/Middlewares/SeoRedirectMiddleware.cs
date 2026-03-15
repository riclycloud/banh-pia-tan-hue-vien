namespace SPage.Middlewares;

/// <summary>
/// Middleware xử lý:
/// 1. Redirect URL có trailing slash
/// 2. Redirect uppercase sang lowercase
/// 3. Canonical URL redirect
/// </summary>
public sealed class SeoRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public SeoRedirectMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";

        // Chỉ áp dụng SEO redirect cho GET requests
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Bỏ qua static files, health check, sitemap, robots, api, admin, account...
        if (IsExcluded(path))
        {
            await _next(context);
            return;
        }

        // Redirect trailing slash (trừ homepage)
        if (path.Length > 1 && path.EndsWith('/'))
        {
            var newPath = path.TrimEnd('/');
            var queryString = context.Request.QueryString;
            context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
            context.Response.Headers.Location = newPath + queryString;
            return;
        }

        // Redirect uppercase sang lowercase
        if (path != path.ToLowerInvariant())
        {
            var queryString = context.Request.QueryString;
            context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
            context.Response.Headers.Location = path.ToLowerInvariant() + queryString;
            return;
        }

        await _next(context);
    }

    private static bool IsExcluded(string path)
    {
        return path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/sitemap.xml", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/robots.txt", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/_", StringComparison.OrdinalIgnoreCase)
            // Bỏ qua toàn bộ khu vực admin để tránh redirect vòng lặp
            || path.Equals("/admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/admin/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/account/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/identity/", StringComparison.OrdinalIgnoreCase);
    }
}
