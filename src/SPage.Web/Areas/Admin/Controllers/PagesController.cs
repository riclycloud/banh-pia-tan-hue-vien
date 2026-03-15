using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Page;
using SPage.Domain.Enums;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class PagesController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugService _slug;
    private readonly IOutputCacheStore _outputCacheStore;
    private readonly ICacheService _cache;
    private readonly ISiteSettingsService _siteSettings;

    // Camelcase for JS interop; case-insensitive for deserialization
    private static readonly JsonSerializerOptions JsonCamel = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly JsonSerializerOptions JsonAny   = new() { PropertyNameCaseInsensitive = true };

    private static string PageSlugCacheKey(string slug) => $"pages:slug:{slug}";

    public PagesController(
        IApplicationDbContext context,
        ISlugService slug,
        IOutputCacheStore outputCacheStore,
        ICacheService cache,
        ISiteSettingsService siteSettings)
    {
        _context = context;
        _slug = slug;
        _outputCacheStore = outputCacheStore;
        _cache = cache;
        _siteSettings = siteSettings;
    }

    // GET /admin/pages
    public async Task<IActionResult> Index()
    {
        var pages = await _context.DynamicPages
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var settings = await _siteSettings.GetAsync(CancellationToken.None);
        ViewBag.HomePageId = settings.HomePageId;

        ViewData["Title"] = "Trang động";
        return View(pages);
    }

    // POST /admin/pages/set-as-homepage/5 (id = 0 hoặc null để bỏ đặt trang chủ)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAsHomepage(int? id, CancellationToken cancellationToken)
    {
        var settings = await _siteSettings.GetAsync(cancellationToken);
        if (id.GetValueOrDefault() > 0)
        {
            var page = await _context.DynamicPages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (page is null) return NotFound();
            if (page.Status != PageStatus.Published)
            {
                TempData["Error"] = "Chỉ có thể đặt trang đã xuất bản làm trang chủ.";
                return RedirectToAction(nameof(Index));
            }
            settings.HomePageId = id;
            TempData["Success"] = $"Đã đặt «{page.Title}» làm trang chủ.";
        }
        else
        {
            settings.HomePageId = null;
            TempData["Success"] = "Đã bỏ đặt trang chủ (trang chủ dùng layout mặc định).";
        }

        await _siteSettings.UpdateAsync(settings, cancellationToken);
        await _outputCacheStore.EvictByTagAsync("pages", cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    // GET /admin/pages/create
    public IActionResult Create()
    {
        ViewData["Title"] = "Thêm trang";
        return View(new PageFormModel());
    }

    // POST /admin/pages/create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageFormModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Thêm trang";
            return View(model);
        }

        // Auto-generate slug if blank
        var slug = string.IsNullOrWhiteSpace(model.Slug)
            ? await _slug.GenerateUniqueAsync(model.Title,
                s => _context.DynamicPages.AnyAsync(p => p.Slug == s))
            : model.Slug.Trim().ToLowerInvariant();

        // Check slug uniqueness
        if (await _context.DynamicPages.AnyAsync(p => p.Slug == slug))
            ModelState.AddModelError(nameof(model.Slug), "Slug này đã tồn tại.");

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Thêm trang";
            return View(model);
        }

        var page = new DynamicPage
        {
            Title           = model.Title.Trim(),
            Slug            = slug,
            Template        = string.IsNullOrWhiteSpace(model.Template) ? "Default" : model.Template.Trim(),
            Status          = model.Status,
            MetaTitle       = string.IsNullOrWhiteSpace(model.MetaTitle) ? model.Title.Trim() : model.MetaTitle.Trim(),
            MetaDescription = model.MetaDescription?.Trim(),
            CanonicalUrl    = model.CanonicalUrl?.Trim(),
            IsIndexable     = model.IsIndexable,
            Sections        = ParseSections(model.SectionsJson),
        };

        _context.DynamicPages.Add(page);
        await _context.SaveChangesAsync(CancellationToken.None);

        await EvictPageCachesAsync(page.Slug, null);

        TempData["Success"] = "Trang đã được tạo thành công.";
        return RedirectToAction(nameof(Edit), new { id = page.Id });
    }

    // GET /admin/pages/edit/5
    public async Task<IActionResult> Edit(int id)
    {
        // Load bằng query để đảm bảo cột JSON Sections được đọc từ DB (FindAsync có thể dùng cache thiếu owned data)
        var page = await _context.DynamicPages
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();
        if (page is null) return NotFound();

        var sections = page.Sections ?? new List<PageSectionData>();
        var sectionsJson = sections.Count > 0
            ? JsonSerializer.Serialize(sections, JsonCamel)
            : "[]";

        ViewData["Title"] = "Sửa trang";
        return View(new PageFormModel
        {
            Id              = page.Id,
            Title           = page.Title,
            Slug            = page.Slug,
            Template        = page.Template,
            Status          = page.Status,
            MetaTitle       = page.MetaTitle,
            MetaDescription = page.MetaDescription,
            CanonicalUrl    = page.CanonicalUrl,
            IsIndexable     = page.IsIndexable,
            SectionsJson    = sectionsJson,
        });
    }

    // POST /admin/pages/edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PageFormModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Sửa trang";
            return View(model);
        }

        var page = await _context.DynamicPages.FindAsync(id);
        if (page is null) return NotFound();

        var oldSlug = page.Slug;
        var slug = string.IsNullOrWhiteSpace(model.Slug)
            ? await _slug.GenerateUniqueAsync(model.Title,
                s => _context.DynamicPages.AnyAsync(p => p.Slug == s && p.Id != id))
            : model.Slug.Trim().ToLowerInvariant();

        // Slug uniqueness (exclude self)
        if (await _context.DynamicPages.AnyAsync(p => p.Slug == slug && p.Id != id))
            ModelState.AddModelError(nameof(model.Slug), "Slug này đã tồn tại.");

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Sửa trang";
            return View(model);
        }

        page.Title           = model.Title.Trim();
        page.Slug            = slug;
        page.Template        = string.IsNullOrWhiteSpace(model.Template) ? "Default" : model.Template.Trim();
        page.Status          = model.Status;
        page.MetaTitle       = string.IsNullOrWhiteSpace(model.MetaTitle) ? model.Title.Trim() : model.MetaTitle.Trim();
        page.MetaDescription = model.MetaDescription?.Trim();
        page.CanonicalUrl    = model.CanonicalUrl?.Trim();
        page.IsIndexable     = model.IsIndexable;
        page.Sections        = ParseSections(model.SectionsJson);

        await _context.SaveChangesAsync(CancellationToken.None);

        await EvictPageCachesAsync(slug, oldSlug);

        TempData["Success"] = "Trang đã được cập nhật.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    // POST /admin/pages/delete/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var page = await _context.DynamicPages.FindAsync(id);
        if (page is null)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = "Không tìm thấy trang." });
            return NotFound();
        }

        var slug = page.Slug;
        _context.DynamicPages.Remove(page);
        await _context.SaveChangesAsync(CancellationToken.None);

        await EvictPageCachesAsync(slug, null);

        var message = "Trang đã được xóa.";
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = true, message });
        TempData["Success"] = message;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Xóa output cache (tag "pages") và cache ứng dụng theo slug(s).</summary>
    private async Task EvictPageCachesAsync(string? currentSlug, string? oldSlug)
    {
        var ct = CancellationToken.None;
        await _outputCacheStore.EvictByTagAsync("pages", ct);
        if (!string.IsNullOrEmpty(currentSlug))
            await _cache.RemoveAsync(PageSlugCacheKey(currentSlug), ct);
        if (!string.IsNullOrEmpty(oldSlug) && oldSlug != currentSlug)
            await _cache.RemoveAsync(PageSlugCacheKey(oldSlug), ct);
    }

    private static List<PageSectionData> ParseSections(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<PageSectionData>>(json, JsonAny) ?? []; }
        catch { return []; }
    }
}

/// <summary>ViewModel cho form tạo/sửa DynamicPage.</summary>
public sealed class PageFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được để trống.")]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Slug { get; set; }

    public string Template { get; set; } = "Default";

    public PageStatus Status { get; set; } = PageStatus.Draft;

    [MaxLength(70)]
    public string? MetaTitle { get; set; }

    [MaxLength(160)]
    public string? MetaDescription { get; set; }

    public string? CanonicalUrl { get; set; }

    public bool IsIndexable { get; set; } = true;

    /// <summary>JSON array of PageSectionData (camelCase) — managed by the section-builder JS.</summary>
    public string? SectionsJson { get; set; }
}
