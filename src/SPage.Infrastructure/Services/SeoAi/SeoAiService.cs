using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Ai;

namespace SPage.Infrastructure.Services.SeoAi;

public sealed class SeoAiService : ISeoAiService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SeoAiService> _logger;

    public SeoAiService(
        IApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<SeoAiService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SeoContentResult> GenerateSeoContentAsync(
        int providerId,
        string topicOrTitle,
        string? existingContent,
        string language = "vi",
        CancellationToken cancellationToken = default)
    {
        var config = providerId > 0
            ? await _context.AiProviderConfigs.FirstOrDefaultAsync(c => c.Id == providerId && c.IsActive, cancellationToken)
            : await _context.AiProviderConfigs.Where(c => c.IsActive).OrderBy(c => c.SortOrder).FirstOrDefaultAsync(cancellationToken);

        if (config is null || string.IsNullOrWhiteSpace(config.ApiKey))
        {
            return new SeoContentResult { Error = "Chưa cấu hình API key AI hoặc provider không tồn tại." };
        }

        var key = config.ProviderKey.ToUpperInvariant();
        if (key == "GEMINI") return await GenerateWithGeminiAsync(config, topicOrTitle, existingContent, language, cancellationToken);
        if (key == "OPENAI") return await GenerateWithOpenAiAsync(config, topicOrTitle, existingContent, language, cancellationToken);
        if (key == "GROK") return await GenerateWithOpenAiCompatibleAsync(config, topicOrTitle, existingContent, language, "https://api.x.ai/v1", cancellationToken);

        return new SeoContentResult { Error = $"Provider '{config.ProviderKey}' chưa được hỗ trợ." };
    }

    private static string BuildPrompt(string topicOrTitle, string? existingContent, string language)
    {
        var langNote = language == "vi" ? "Tiếng Việt." : "Vietnamese.";
        var sb = new StringBuilder();
        sb.AppendLine("Bạn là chuyên gia SEO. Nhiệm vụ: tạo nội dung chuẩn SEO cho bài viết.");
        sb.AppendLine($"Ngôn ngữ: {langNote}");
        sb.AppendLine();
        sb.AppendLine("Chủ đề / tiêu đề bài viết:");
        sb.AppendLine(topicOrTitle);
        if (!string.IsNullOrWhiteSpace(existingContent))
        {
            sb.AppendLine();
            sb.AppendLine("Nội dung gốc (có thể dùng để tóm tắt hoặc mở rộng):");
            sb.AppendLine(existingContent.Length > 4000 ? existingContent[..4000] + "..." : existingContent);
        }
        sb.AppendLine();
        sb.AppendLine("Trả về ĐÚNG theo JSON sau (không thêm markdown, chỉ JSON thuần):");
        sb.AppendLine("{\"title\":\"Tiêu đề hấp dẫn tối đa 70 ký tự\",\"metaTitle\":\"Meta title cho SEO\",\"metaDescription\":\"Mô tả 150-160 ký tự\",\"excerpt\":\"Đoạn tóm tắt 1-2 câu\",\"contentSuggestion\":\"Đoạn mở đầu hoặc gợi ý nội dung mở rộng (HTML hoặc text)\"}");
        return sb.ToString();
    }

    private async Task<SeoContentResult> GenerateWithGeminiAsync(AiProviderConfig config, string topicOrTitle, string? existingContent, string language, CancellationToken ct)
    {
        var model = string.IsNullOrWhiteSpace(config.ModelName) ? "gemini-1.5-flash" : config.ModelName;
        var baseUrl = string.IsNullOrWhiteSpace(config.BaseUrl) ? "https://generativelanguage.googleapis.com/v1beta" : config.BaseUrl.TrimEnd('/');
        var url = $"{baseUrl}/models/{model}:generateContent?key={Uri.EscapeDataString(config.ApiKey ?? "")}";

        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = BuildPrompt(topicOrTitle, existingContent, language) } } } },
            generationConfig = new { temperature = 0.4, maxOutputTokens = 2048 }
        };

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(url, body, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
            return ParseJsonResponse(text);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini API error");
            return new SeoContentResult { Error = ex.Message };
        }
    }

    private async Task<SeoContentResult> GenerateWithOpenAiAsync(AiProviderConfig config, string topicOrTitle, string? existingContent, string language, CancellationToken ct)
    {
        return await GenerateWithOpenAiCompatibleAsync(config, topicOrTitle, existingContent, language, config.BaseUrl ?? "https://api.openai.com/v1", ct);
    }

    private async Task<SeoContentResult> GenerateWithOpenAiCompatibleAsync(AiProviderConfig config, string topicOrTitle, string? existingContent, string language, string baseUrl, CancellationToken ct)
    {
        var model = string.IsNullOrWhiteSpace(config.ModelName) ? "gpt-4o-mini" : config.ModelName;
        var url = $"{baseUrl.TrimEnd('/')}/chat/completions";

        var messages = new object[]
        {
            new { role = "system", content = "You are an SEO expert. Reply only with valid JSON, no markdown." },
            new { role = "user", content = BuildPrompt(topicOrTitle, existingContent, language) }
        };

        var body = new
        {
            model,
            messages,
            response_format = new { type = "json_object" },
            max_tokens = 2048
        };

        try
        {
            var client = _httpClientFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Add("Authorization", "Bearer " + (config.ApiKey ?? ""));
            req.Content = JsonContent.Create(body);
            var response = await client.SendAsync(req, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            var text = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            return ParseJsonResponse(text);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenAI-compatible API error");
            return new SeoContentResult { Error = ex.Message };
        }
    }

    private static SeoContentResult ParseJsonResponse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new SeoContentResult { Error = "API không trả về nội dung." };

        text = text.Trim();
        if (text.StartsWith("```")) text = text.Replace("```json", "").Replace("```", "").Trim();

        try
        {
            var doc = JsonDocument.Parse(text);
            var r = doc.RootElement;
            return new SeoContentResult
            {
                Title = r.TryGetProperty("title", out var t) ? t.GetString() : null,
                MetaTitle = r.TryGetProperty("metaTitle", out var mt) ? mt.GetString() : null,
                MetaDescription = r.TryGetProperty("metaDescription", out var md) ? md.GetString() : null,
                Excerpt = r.TryGetProperty("excerpt", out var e) ? e.GetString() : null,
                ContentSuggestion = r.TryGetProperty("contentSuggestion", out var cs) ? cs.GetString() : null
            };
        }
        catch
        {
            return new SeoContentResult { Error = "Không phân tích được phản hồi JSON từ AI." };
        }
    }
}
