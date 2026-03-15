using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SPage.Application.Features.VideoLibrary.Commands;
using SPage.Application.Features.VideoLibrary.Queries;
using SPage.Domain.Enums;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class VideoLibraryController : Controller
{
    private readonly IMediator _mediator;
    public VideoLibraryController(IMediator mediator) => _mediator = mediator;

    // GET /admin/video-library
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? categoryId, int? platform)
    {
        var videos = await _mediator.Send(new GetAllVideosQuery(search, categoryId, platform));
        var cats   = await _mediator.Send(new GetAllVideoCategoriesQuery());

        ViewBag.Search     = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Platform   = platform;
        ViewBag.Categories = new SelectList(cats, "Id", "Name", categoryId);
        ViewBag.Platforms  = new SelectList(
            Enum.GetValues<VideoPlatform>().Select(p => new { Id = (int)p, Name = p.ToString() }),
            "Id", "Name", platform);

        return View(videos);
    }

    // GET /admin/video-library/create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateViewBag();
        return View("CreateEdit", new VideoFormModel());
    }

    // POST /admin/video-library/create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VideoFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBag();
            return View("CreateEdit", model);
        }

        var id = await _mediator.Send(new CreateVideoCommand(
            model.Title, model.Platform, model.VideoUrl,
            model.VideoId, model.ThumbnailUrl, model.Description,
            model.Duration, model.CategoryId, model.TagsCsv,
            model.MetaTitle, model.MetaDescription,
            model.IsIndexable, model.IsFeatured, model.IsPublished, model.Slug));

        TempData["Success"] = "Thêm video thành công!";
        return RedirectToAction(nameof(Edit), new { id });
    }

    // GET /admin/video-library/edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var videos = await _mediator.Send(new GetAllVideosQuery());
        var video  = videos.FirstOrDefault(v => v.Id == id);
        if (video is null) return NotFound();

        // Load full details
        var all = await _mediator.Send(new GetAllVideosQuery());
        await PopulateViewBag();

        // Re-query full entity via a by-id approach (using admin query ignoring soft-delete)
        var model = await BuildFormModel(id);
        if (model is null) return NotFound();

        return View("CreateEdit", model);
    }

    // POST /admin/video-library/edit/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VideoFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBag();
            return View("CreateEdit", model);
        }

        await _mediator.Send(new UpdateVideoCommand(
            id, model.Title, model.Platform ?? VideoPlatform.YouTube, model.VideoUrl,
            model.VideoId ?? string.Empty, model.ThumbnailUrl, model.Description,
            model.Duration, model.CategoryId, model.TagsCsv,
            model.MetaTitle, model.MetaDescription,
            model.IsIndexable, model.IsFeatured, model.IsPublished, model.Slug));

        TempData["Success"] = "Cập nhật video thành công!";
        return RedirectToAction(nameof(Edit), new { id });
    }

    // POST /admin/video-library/delete/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteVideoCommand(id));
        TempData["Success"] = "Đã xóa video.";
        return RedirectToAction(nameof(Index));
    }

    // GET /admin/video-library/api-preview?platform=0&videoId=dQw4w9WgXcQ
    [HttpGet]
    public IActionResult ApiPreview(int platform, string videoId, string? videoUrl)
    {
        var p = (VideoPlatform)platform;
        var thumb = p == VideoPlatform.YouTube
            ? $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg"
            : null;
        var embed = p switch
        {
            VideoPlatform.YouTube  => $"https://www.youtube.com/embed/{videoId}?rel=0",
            VideoPlatform.Facebook => $"https://www.facebook.com/plugins/video.php?href={Uri.EscapeDataString(videoUrl ?? "")}&show_text=0",
            VideoPlatform.TikTok   => $"https://www.tiktok.com/embed/v2/{videoId}",
            _                      => videoUrl
        };
        return Json(new { thumbnailUrl = thumb, embedUrl = embed });
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    private async Task PopulateViewBag()
    {
        var cats = await _mediator.Send(new GetAllVideoCategoriesQuery());
        ViewBag.Categories = new SelectList(cats, "Id", "Name");
        ViewBag.Platforms  = Enum.GetValues<VideoPlatform>()
            .Select(p => new SelectListItem(p.ToString(), ((int)p).ToString()))
            .ToList();
    }

    private async Task<VideoFormModel?> BuildFormModel(int id)
    {
        // Use GetAllVideosQuery and find by id (admin view sees all)
        var list = await _mediator.Send(new GetAllVideosQuery());
        var v    = list.FirstOrDefault(x => x.Id == id);
        if (v is null) return null;

        return new VideoFormModel
        {
            Id           = v.Id,
            Title        = v.Title,
            Slug         = v.Slug,
            Platform     = v.Platform,
            VideoId      = v.VideoId,
            VideoUrl     = "", // VideoUrl not in summary — will be blank (user can re-enter)
            ThumbnailUrl = v.ThumbnailUrl == v.ThumbnailUrl ? v.ThumbnailUrl : null,
            IsPublished  = v.IsPublished,
            IsFeatured   = v.IsFeatured,
        };
    }
}

public sealed class VideoFormModel
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    [System.ComponentModel.DataAnnotations.MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(2000)]
    public string? Description { get; set; }

    public VideoPlatform? Platform { get; set; } = VideoPlatform.YouTube;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "URL video là bắt buộc")]
    [System.ComponentModel.DataAnnotations.MaxLength(1000)]
    public string VideoUrl { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string? VideoId { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(1000)]
    public string? ThumbnailUrl { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string? Duration { get; set; }

    public int? CategoryId { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? TagsCsv { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string? MetaTitle { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? MetaDescription { get; set; }

    public bool IsIndexable { get; set; } = true;
    public bool IsFeatured  { get; set; }
    public bool IsPublished { get; set; }
}
