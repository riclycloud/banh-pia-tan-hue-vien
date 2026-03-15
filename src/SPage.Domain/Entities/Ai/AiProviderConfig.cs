namespace SPage.Domain.Entities.Ai;

/// <summary>
/// Cấu hình API key cho từng nhà cung cấp AI (Gemini, OpenAI, Grok...).
/// Dùng cho công cụ viết bài chuẩn SEO trong Admin.
/// </summary>
public sealed class AiProviderConfig
{
    public int Id { get; set; }

    /// <summary>Mã nhà cung cấp: Gemini, OpenAI, Grok.</summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>Tên hiển thị trong Admin.</summary>
    public string? DisplayName { get; set; }

    public string? ApiKey { get; set; }

    /// <summary>Base URL tùy chỉnh (để trống dùng mặc định theo provider).</summary>
    public string? BaseUrl { get; set; }

    /// <summary>Model: gemini-1.5-flash, gpt-4o-mini, grok-2...</summary>
    public string? ModelName { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
