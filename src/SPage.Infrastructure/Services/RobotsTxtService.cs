using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SPage.Application.Common.Interfaces;

namespace SPage.Infrastructure.Services;

/// <summary>
/// Robots.txt chuẩn SEO: User-agent, Allow/Disallow, Sitemap tuyệt đối.
/// Nếu cấu hình Seo:RobotsContent có sẵn thì dùng nguyên bản; không thì sinh theo chuẩn.
/// </summary>
public sealed class RobotsTxtService : IRobotsTxtService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public RobotsTxtService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public Task<string> GetContentAsync(CancellationToken cancellationToken = default)
    {
        var custom = _configuration["Seo:RobotsContent"]?.Trim();
        if (!string.IsNullOrEmpty(custom))
            return Task.FromResult(NormalizeLineEndings(custom));

        var baseUrl = GetBaseUrl();
        var disallowPaths = new List<string> { "/admin/", "/account/", "/identity/", "/api/" };
        var extra = _configuration.GetSection("Seo:RobotsDisallow").Get<string[]>();
        if (extra?.Length > 0)
        {
            foreach (var path in extra)
            {
                var p = (path ?? "").Trim();
                if (p.Length > 0 && !disallowPaths.Contains(p))
                    disallowPaths.Add(p.StartsWith("/", StringComparison.Ordinal) ? p : "/" + p);
            }
        }
        var lines = new List<string>
        {
            "# https://www.robotstxt.org/robotstxt.html",
            "User-agent: *",
            "Allow: /"
        };
        foreach (var path in disallowPaths)
            lines.Add($"Disallow: {path}");
        lines.Add("");
        lines.Add($"Sitemap: {baseUrl.TrimEnd('/')}/sitemap.xml");

        return Task.FromResult(string.Join("\n", lines));
    }

    private string GetBaseUrl()
    {
        var configured = _configuration["Seo:BaseUrl"]?.Trim();
        if (!string.IsNullOrEmpty(configured))
            return configured.TrimEnd('/');
        var request = _httpContextAccessor.HttpContext?.Request;
        return request is not null ? $"{request.Scheme}://{request.Host}" : "https://localhost";
    }

    private static string NormalizeLineEndings(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;
        return content.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd();
    }
}
