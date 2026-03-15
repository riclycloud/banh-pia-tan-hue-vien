using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SPage.Application.Features.Categories.Queries;
using SPage.Application.Features.Posts.Queries;
using SPage.Middlewares;
using SPage.ViewModels.Seo;

namespace SPage.Controllers;

public sealed class PostsController : Controller
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator) => _mediator = mediator;

    [OutputCache(PolicyName = "PostsList")]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        [FromQuery] int? category = null,
        [FromQuery] string? tag = null,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetPostsQuery(page, 10, category, tag, q), cancellationToken);

        if (category.HasValue)
        {
            var categoryInfo = await _mediator.Send(new GetCategoryByIdQuery(category.Value), cancellationToken);
            ViewBag.Category = categoryInfo;
        }
        else
        {
            ViewBag.Category = null;
        }

        ViewData["Seo"] = new SeoViewModel
        {
            Title = $"{GetLocalizedTitle()} — {Request.HttpContext.GetCurrentCulture()}",
            MetaDescription = "Tổng hợp bài viết mới nhất.",
            CanonicalUrl = Url.Action("Index", "Posts", new { page }, Request.Scheme),
            IsIndexable = page == 1 && string.IsNullOrEmpty(q)
        };

        return View(result);
    }

    [OutputCache(PolicyName = "PostDetail")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        var post = await _mediator.Send(new GetPostBySlugQuery(slug), cancellationToken);
        if (post is null) return NotFound();

        var canonicalUrl = post.CanonicalUrl
            ?? Url.Action("Detail", "Posts", new { slug }, Request.Scheme)!;

        ViewData["Seo"] = new SeoViewModel
        {
            Title = post.MetaTitle,
            MetaDescription = post.MetaDescription,
            CanonicalUrl = canonicalUrl,
            IsIndexable = post.IsIndexable,
            OgType = "article",
            OgImage = post.FeaturedImageUrl,
            OgImageAlt = post.FeaturedImageAlt,
            SchemaOrgJson = BuildArticleSchema(post, canonicalUrl),
            HreflangUrls = BuildHreflangUrls(slug)
        };

        return View(post);
    }

    private Dictionary<string, string> BuildHreflangUrls(string slug)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return new Dictionary<string, string>
        {
            ["vi"] = $"{baseUrl}/vi/{slug}",
            ["en"] = $"{baseUrl}/en/{slug}",
            ["x-default"] = $"{baseUrl}/vi/{slug}"
        };
    }

    private string BuildArticleSchema(
        Application.Features.Posts.DTOs.PostDetailDto post,
        string canonicalUrl)
    {
        return JsonSerializer.Serialize(new
        {
            @context = "https://schema.org",
            @type = post.SchemaType,
            headline = post.Title,
            description = post.MetaDescription,
            image = post.FeaturedImageUrl,
            datePublished = post.PublishedAt?.ToString("O"),
            dateModified = post.PublishedAt?.ToString("O"),
            url = canonicalUrl,
            author = new
            {
                @type = "Person",
                name = post.CreatedBy
            },
            publisher = new
            {
                @type = "Organization",
                name = "SPage",
                logo = new { @type = "ImageObject", url = $"{Request.Scheme}://{Request.Host}/images/logo.webp" }
            }
        }, new JsonSerializerOptions { WriteIndented = false });
    }

    private string GetLocalizedTitle() => "Tin tức & Blog";
}
