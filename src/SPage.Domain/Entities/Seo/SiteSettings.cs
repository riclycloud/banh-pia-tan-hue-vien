namespace SPage.Domain.Entities.Seo;

/// <summary>
/// Cấu hình chung cho toàn bộ website.
/// Một bản ghi duy nhất (Id = 1).
/// </summary>
public sealed class SiteSettings
{
    public int Id { get; set; }

    public string SiteName { get; set; } = "SPage";
    public string? SiteTagline { get; set; }
    public string? DefaultMetaDescription { get; set; }

    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    /// <summary>Email nhận thông tin liên hệ từ form contact (nếu null sẽ dùng Email)</summary>
    public string? ContactEmail { get; set; }

    public string? FacebookPageUrl { get; set; }

    /// <summary>Google Analytics Measurement ID (VD: G-XXXXXXX)</summary>
    public string? GoogleAnalyticsId { get; set; }

    /// <summary>HTML tùy chỉnh chèn vào &lt;head&gt; (vd: meta, pixel, verify)</summary>
    public string? CustomHeadHtml { get; set; }

    /// <summary>HTML tùy chỉnh chèn trước &lt;/body&gt; (vd: script tracking)</summary>
    public string? CustomFooterHtml { get; set; }

    /// <summary>Id trang động dùng làm trang chủ (null = dùng layout mặc định).</summary>
    public int? HomePageId { get; set; }
}

