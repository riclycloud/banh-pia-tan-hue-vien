using MediatR;
using Microsoft.AspNetCore.Mvc;
using SPage.Application.Features.Pages.Queries;
using SPage.ViewModels.Seo;

namespace SPage.Controllers;

public sealed class SlugController : Controller
{
    private readonly IMediator _mediator;

    public SlugController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Chỉ xử lý slug là trang (Page). Các path cố định (san-pham, bai-viet, lien-he, ...) phải do MapControllerRoute xử lý, không vào đây.
    /// </summary>
    [Route("{slug:regex(^(?!admin$)(?!account$)(?!san-pham$)(?!bai-viet$)(?!lien-he$)(?!dai-ly$)(?!video$)(?!contact$)(?!posts$)(?!products$)(?!home$)(?!api$)(?!blog$).+)}")]
    public async Task<IActionResult> Resolve(string slug, CancellationToken ct)
    {
        slug = (slug ?? string.Empty).Trim('/').ToLowerInvariant();

        var page = await _mediator.Send(new GetPageBySlugQuery(slug), ct);
        if (page is not null)
        {
            var canonicalUrl = page.CanonicalUrl
                ?? Url.Action("Resolve", "Slug", new { slug }, Request.Scheme);
            var title = !string.IsNullOrWhiteSpace(page.MetaTitle) ? page.MetaTitle : page.Title;
            var schemaJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                @context = "https://schema.org",
                @type = "WebPage",
                name = title,
                url = canonicalUrl,
                description = page.MetaDescription
            });

            ViewData["Seo"] = new SeoViewModel
            {
                Title = title,
                MetaDescription = page.MetaDescription,
                CanonicalUrl = canonicalUrl,
                IsIndexable = page.IsIndexable,
                OgType = "website",
                SchemaOrgJson = schemaJson
            };

            var viewName = $"~/Views/PageTemplates/{page.Template}.cshtml";
            return View(viewName, page);
        }

        return NotFound();
    }
}
