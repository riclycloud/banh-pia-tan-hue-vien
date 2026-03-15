namespace SPage.Middlewares;

/// <summary>
/// Hỗ trợ URL-based culture: /{culture}/...
/// Ví dụ: /vi/blog, /en/blog
/// </summary>
public sealed class LocalizationCultureMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> SupportedCultures = ["vi", "en"];

    public LocalizationCultureMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var segments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments?.Length > 0 && SupportedCultures.Contains(segments[0]))
        {
            var culture = segments[0];

            // Rewrite path: /vi/blog/slug → /blog/slug
            var remainingPath = "/" + string.Join("/", segments.Skip(1));
            context.Request.Path = remainingPath.Length > 1 ? remainingPath : "/";

            // Override culture từ URL (ưu tiên hơn cookie)
            var cultureInfo = new System.Globalization.CultureInfo(culture);
            System.Globalization.CultureInfo.CurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.CurrentUICulture = cultureInfo;
        }

        await _next(context);
    }
}

/// <summary>Extension method để đọc culture hiện tại từ HttpContext</summary>
public static class HttpContextExtensions
{
    public static string GetCurrentCulture(this HttpContext context)
        => System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
}
