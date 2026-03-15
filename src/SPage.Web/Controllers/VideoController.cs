using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SPage.Application.Features.VideoLibrary.Queries;
using SPage.Domain.Enums;
using SPage.ViewModels.Seo;

namespace SPage.Controllers;

public sealed class VideoController : Controller
{
    private readonly IMediator _mediator;
    public VideoController(IMediator mediator) => _mediator = mediator;

    // GET /video — hiển thị theo section theo danh mục
    [OutputCache(PolicyName = "VideosList")]
    public async Task<IActionResult> Index(
        [FromQuery] string? q = null,
        CancellationToken   ct = default)
    {
        var result = await _mediator.Send(
            new GetPublishedVideosGroupedByCategoryQuery(Search: q, MaxVideosPerCategory: 12, CategorySlug: null), ct);

        ViewData["Seo"] = new SeoViewModel
        {
            Title           = "Thư viện Video",
            MetaDescription = "Xem các video mới nhất theo chuyên mục: tin tức, giải trí, TVC quảng cáo và nhiều hơn nữa.",
            CanonicalUrl    = Url.Action("Index", "Video", null, Request.Scheme),
            IsIndexable     = string.IsNullOrEmpty(q),
            OgType          = "website"
        };

        return View(result);
    }

    /// <summary>Phân giải /video/{slug}: ưu tiên slug video (xem video), không có thì xem là slug danh mục.</summary>
    [OutputCache(PolicyName = "VideosList")]
    public async Task<IActionResult> BySlug(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();

        // Ưu tiên video trước — slug xem video phải hoạt động
        var video = await _mediator.Send(new GetVideoBySlugQuery(slug), ct);
        if (video is not null)
            return await DetailCore(video, slug, ct);

        var category = await _mediator.Send(new GetVideoCategoryBySlugQuery(slug), ct);
        if (category is not null)
        {
            var result = await _mediator.Send(
                new GetPublishedVideosGroupedByCategoryQuery(Search: null, MaxVideosPerCategory: 24, CategorySlug: slug), ct);
            if (result.Sections.Count == 0)
                return NotFound();
            ViewData["Seo"] = new SeoViewModel
            {
                Title           = $"{category.Name} — Thư viện Video",
                MetaDescription = category.Description ?? $"Xem video chuyên mục {category.Name}.",
                CanonicalUrl    = Url.Action("BySlug", "Video", new { slug }, Request.Scheme),
                IsIndexable     = true,
                OgType          = "website"
            };
            return View("Index", result);
        }

        return NotFound();
    }

    // GET /video/detail/{slug} — direct link (slug rewrite có thể trỏ /video/{slug} → BySlug)
    [OutputCache(PolicyName = "VideoDetail")]
    public async Task<IActionResult> Detail(string slug, CancellationToken ct = default)
    {
        var video = await _mediator.Send(new GetVideoBySlugQuery(slug), ct);
        if (video is null) return NotFound();
        return await DetailCore(video, slug, ct);
    }

    private async Task<IActionResult> DetailCore(
        SPage.Application.Features.VideoLibrary.DTOs.VideoDetailDto video,
        string slug,
        CancellationToken ct)
    {
        var related = await _mediator.Send(
            new GetPublishedVideosQuery(video.CategoryId, null, 1, 4), ct);

        ViewBag.RelatedVideos = related.Items
            .Where(v => v.Id != video.Id)
            .Take(4)
            .ToList();

        var canonicalUrl = video.CanonicalUrl
            ?? Url.Action("Detail", "Video", new { slug }, Request.Scheme)!;

        var thumbnail = video.ThumbnailUrl;

        ViewData["Seo"] = new SeoViewModel
        {
            Title           = video.MetaTitle ?? video.Title,
            MetaDescription = video.MetaDescription ?? video.Description,
            CanonicalUrl    = canonicalUrl,
            IsIndexable     = video.IsIndexable,
            OgType          = "video.other",
            OgImage         = thumbnail,
            OgImageAlt      = video.Title,
            SchemaOrgJson   = BuildVideoSchema(video, canonicalUrl, thumbnail),
            HreflangUrls    = new Dictionary<string, string>
            {
                ["vi"]    = Url.Action("Detail","Video",new{slug},Request.Scheme)!,
                ["x-default"] = Url.Action("Detail","Video",new{slug},Request.Scheme)!
            }
        };

        return View("Detail", video);
    }

    // ── Schema.org VideoObject ────────────────────────────────────────────────
    private static string BuildVideoSchema(
        SPage.Application.Features.VideoLibrary.DTOs.VideoDetailDto v,
        string canonicalUrl,
        string? thumbnail)
    {
        var schema = new Dictionary<string, object?>
        {
            ["@context"]     = "https://schema.org",
            ["@type"]        = "VideoObject",
            ["name"]         = v.Title,
            ["description"]  = v.Description ?? v.Title,
            ["thumbnailUrl"] = thumbnail,
            ["uploadDate"]   = v.PublishedAt?.ToString("O") ?? v.CreatedAt.ToString("O"),
            ["embedUrl"]     = v.EmbedUrl,
            ["contentUrl"]   = v.VideoUrl,
            ["url"]          = canonicalUrl
        };

        if (!string.IsNullOrEmpty(v.Duration))
            schema["duration"] = v.Duration;

        if (v.ViewCount > 0)
            schema["interactionStatistic"] = new Dictionary<string, object>
            {
                ["@type"]            = "InteractionCounter",
                ["interactionType"]  = "https://schema.org/WatchAction",
                ["userInteractionCount"] = v.ViewCount
            };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions
        {
            WriteIndented         = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }
}
