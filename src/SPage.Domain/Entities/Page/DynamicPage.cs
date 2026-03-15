using SPage.Domain.Common;
using SPage.Domain.Enums;

namespace SPage.Domain.Entities.Page;

public sealed class DynamicPage : BaseAuditableEntity, ISeoEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool IsIndexable { get; set; } = true;
    public PageStatus Status { get; set; } = PageStatus.Draft;

    // JSON column — toàn bộ layout lưu dưới dạng JSON (EF Core 8 ToJson())
    public List<PageSectionData> Sections { get; set; } = [];

    // Razor view template name (e.g. "Default", "LandingPage", "Contact")
    public string Template { get; set; } = "Default";
}

/// <summary>Value object — serialized thành JSON column trong SQL Server</summary>
public sealed class PageSectionData
{
    /// <summary>Hero | TextBlock | Gallery | ContactForm | ProductList | Testimonials</summary>
    public string SectionType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
    public string? CssClass { get; set; }

    private Dictionary<string, string>? _data;

    /// <summary>Flexible key-value data cho từng loại section. Luôn khác null (kể cả khi section lấy từ thư viện có data null).</summary>
    public Dictionary<string, string> Data
    {
        get => _data ??= [];
        set => _data = value ?? [];
    }
}
