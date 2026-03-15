namespace SPage.Application.Common.Interfaces;

/// <summary>
/// Kết quả gợi ý SEO từ AI (tiêu đề, meta, tóm tắt, nội dung).
/// </summary>
public sealed class SeoContentResult
{
    public string? Title { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Excerpt { get; set; }
    public string? ContentSuggestion { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Gọi AI (Gemini, ChatGPT, Grok...) để gợi ý nội dung chuẩn SEO.
/// </summary>
public interface ISeoAiService
{
    /// <summary>
    /// Sinh gợi ý tiêu đề, meta description, excerpt (và có thể mở rộng nội dung) từ chủ đề hoặc nội dung gốc.
    /// </summary>
    /// <param name="providerId">Id của AiProviderConfig (0 = dùng provider active đầu tiên).</param>
    /// <param name="topicOrTitle">Chủ đề hoặc tiêu đề bài viết.</param>
    /// <param name="existingContent">Nội dung có sẵn (tùy chọn) để AI tóm tắt/chuẩn hóa SEO.</param>
    /// <param name="language">Ngôn ngữ đích, mặc định "vi".</param>
    Task<SeoContentResult> GenerateSeoContentAsync(
        int providerId,
        string topicOrTitle,
        string? existingContent,
        string language = "vi",
        CancellationToken cancellationToken = default);
}
