using SPage.Domain.Common;

namespace SPage.Domain.Entities.Page;

/// <summary>Lưu trữ cấu hình section component tái sử dụng (thư viện section)</summary>
public sealed class SectionTemplate : BaseAuditableEntity
{
    /// <summary>Tên hiển thị trong thư viện, e.g. "Hero trang chủ"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Loại section: Hero | Gallery | ContactForm | Testimonials | ...</summary>
    public string SectionType { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Dữ liệu cấu hình dạng JSON (Dictionary&lt;string,string&gt;)</summary>
    public string DataJson { get; set; } = "{}";

    public string? CssClass { get; set; }
}
