namespace SPage.Application.Common.Interfaces;

/// <summary>
/// Sinh nội dung robots.txt chuẩn (RFC 9309 / sitemaps.org), hỗ trợ override từ cấu hình.
/// </summary>
public interface IRobotsTxtService
{
    /// <summary>Nội dung robots.txt (UTF-8), đã bao gồm Sitemap với URL tuyệt đối.</summary>
    Task<string> GetContentAsync(CancellationToken cancellationToken = default);
}
