using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Ai;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class SeoAiToolController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly ISeoAiService _seoAiService;

    public SeoAiToolController(IApplicationDbContext context, ISeoAiService seoAiService)
    {
        _context = context;
        _seoAiService = seoAiService;
    }

    public async Task<IActionResult> Index()
    {
        var providers = await _context.AiProviderConfigs
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new { c.Id, c.ProviderKey, c.DisplayName })
            .ToListAsync(HttpContext.RequestAborted);

        ViewData["Title"] = "Công cụ viết bài chuẩn SEO (AI)";
        ViewBag.Providers = providers;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSeoContent([FromBody] GenerateSeoRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.TopicOrTitle))
        {
            return Json(new { success = false, error = "Vui lòng nhập chủ đề hoặc tiêu đề." });
        }

        var result = await _seoAiService.GenerateSeoContentAsync(
            request.ProviderId,
            request.TopicOrTitle.Trim(),
            string.IsNullOrWhiteSpace(request.ExistingContent) ? null : request.ExistingContent.Trim(),
            request.Language ?? "vi",
            cancellationToken);

        if (!string.IsNullOrEmpty(result.Error))
            return Json(new { success = false, error = result.Error });

        return Json(new
        {
            success = true,
            title = result.Title,
            metaTitle = result.MetaTitle,
            metaDescription = result.MetaDescription,
            excerpt = result.Excerpt,
            contentSuggestion = result.ContentSuggestion
        });
    }

    /// <summary>Lưu nội dung AI thành bản nháp bài viết, chuyển sang form tạo bài để điều chỉnh và lưu.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SaveAsPost([FromForm] SeoAiDraftModel draft)
    {
        if (string.IsNullOrWhiteSpace(draft?.Title))
        {
            TempData["Error"] = "Chưa có nội dung từ AI. Hãy tạo gợi ý SEO trước.";
            return RedirectToAction(nameof(Index));
        }
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            Title = draft.Title.Trim(),
            Content = (draft.Content ?? "").Trim(),
            Excerpt = (draft.Excerpt ?? "").Trim(),
            MetaTitle = (draft.MetaTitle ?? "").Trim(),
            MetaDescription = (draft.MetaDescription ?? "").Trim()
        });
        TempData["SeoAiDraft"] = json;
        return RedirectToAction("Create", "Posts");
    }
}

public sealed class GenerateSeoRequest
{
    public int ProviderId { get; set; }
    public string? TopicOrTitle { get; set; }
    public string? ExistingContent { get; set; }
    public string? Language { get; set; }
}

public sealed class SeoAiDraftModel
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Excerpt { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}
