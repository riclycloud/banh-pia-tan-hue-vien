using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SPage.Application.Features.Pages.Queries;
using SPage.ViewModels.Seo;

namespace SPage.Controllers;

public sealed class PagesController : Controller
{
    private readonly IMediator _mediator;

    public PagesController(IMediator mediator) => _mediator = mediator;

    [OutputCache(PolicyName = "PageDetail")]
    public async Task<IActionResult> Show(string slug, CancellationToken cancellationToken)
    {
        var page = await _mediator.Send(new GetPageBySlugQuery(slug), cancellationToken);
        if (page is null) return NotFound();

        var canonicalUrl = page.CanonicalUrl
            ?? Url.Action("Show", "Pages", new { slug }, Request.Scheme);

        // JSON-LD WebPage schema
        var schemaJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            @context    = "https://schema.org",
            @type       = "WebPage",
            name        = !string.IsNullOrWhiteSpace(page.MetaTitle) ? page.MetaTitle : page.Title,
            url         = canonicalUrl,
            description = page.MetaDescription
        });

        ViewData["Seo"] = new SeoViewModel
        {
            Title           = !string.IsNullOrWhiteSpace(page.MetaTitle) ? page.MetaTitle : page.Title,
            MetaDescription = page.MetaDescription,
            CanonicalUrl    = canonicalUrl,
            IsIndexable     = page.IsIndexable,
            OgType          = "website",
            SchemaOrgJson   = schemaJson
        };

        // Render theo template động
        var viewName = $"~/Views/PageTemplates/{page.Template}.cshtml";
        return View(viewName, page);
    }
}
