using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Pages.Queries;
using SPage.Application.Features.Posts.Queries;
using SPage.Application.Features.ProductCategories.Queries;
using SPage.Application.Features.Products.Queries;
using SPage.Models;
using SPage.ViewModels.Seo;

namespace SPage.Controllers;

public sealed class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ISiteSettingsService _siteSettings;
    private readonly IMediator _mediator;

    public HomeController(ILogger<HomeController> logger, ISiteSettingsService siteSettings, IMediator mediator)
    {
        _logger = logger;
        _siteSettings = siteSettings;
        _mediator = mediator;
    }

    [OutputCache(PolicyName = "PageDetail")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var settings = await _siteSettings.GetAsync(cancellationToken);
        var siteUrl  = $"{Request.Scheme}://{Request.Host}";

        // Nếu đã chọn trang động làm trang chủ thì hiển thị nội dung trang đó
        if (settings.HomePageId.HasValue)
        {
            var page = await _mediator.Send(new GetPageByIdQuery(settings.HomePageId.Value), cancellationToken);
            if (page is not null)
            {
                var canonicalUrl = page.CanonicalUrl ?? siteUrl;
                var schemaJson = JsonSerializer.Serialize(new
                {
                    @context = "https://schema.org",
                    @type   = "WebPage",
                    name    = page.MetaTitle,
                    url     = canonicalUrl,
                    description = page.MetaDescription
                });
                ViewData["Seo"] = new SeoViewModel
                {
                    Title           = page.MetaTitle,
                    MetaDescription = page.MetaDescription,
                    CanonicalUrl    = canonicalUrl,
                    IsIndexable     = page.IsIndexable,
                    OgType          = "website",
                    SchemaOrgJson   = schemaJson
                };
                var viewName = $"~/Views/PageTemplates/{page.Template}.cshtml";
                return View(viewName, page);
            }
        }

        // Trang chủ mặc định (layout + sản phẩm nổi bật, tin tức)
        var titleParts = new[] { settings.SiteName, settings.SiteTagline }
            .Where(s => !string.IsNullOrWhiteSpace(s));
        var title = string.Join(" — ", titleParts);
        if (string.IsNullOrWhiteSpace(title))
            title = !string.IsNullOrWhiteSpace(settings.SiteName) ? settings.SiteName : "Trang chủ";

        ViewData["Seo"] = new SeoViewModel
        {
            Title           = title,
            MetaDescription = settings.DefaultMetaDescription,
            CanonicalUrl    = siteUrl,
            IsIndexable     = true,
            OgType          = "website",
            SchemaOrgJson   = JsonSerializer.Serialize(new
            {
                @context = "https://schema.org",
                @type    = "WebSite",
                name     = settings.SiteName,
                url      = siteUrl
            })
        };

        var productsResult = await _mediator.Send(
            new GetProductsQuery(Page: 1, PageSize: 8, SortBy: "newest"), cancellationToken);
        ViewBag.FeaturedProducts = productsResult.Items;

        var categories = await _mediator.Send(new GetProductCategoriesQuery(), cancellationToken);
        ViewBag.ProductCategories = categories.Where(c => c.ProductCount > 0).ToList();

        var productsSection6 = await _mediator.Send(
            new GetProductsQuery(Page: 1, PageSize: 24, SortBy: "newest"), cancellationToken);
        ViewBag.ProductsSection6 = productsSection6.Items;

        var postsResult = await _mediator.Send(
            new GetPostsQuery(Page: 1, PageSize: 4), cancellationToken);
        ViewBag.RecentPosts = postsResult.Items;

        return View();
    }

    public IActionResult Privacy()
    {
        ViewData["Title"] = "Chính sách bảo mật";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Trang hiển thị khi response 4xx/5xx (404 → NotFound.cshtml, style trang chủ).
    /// Gọi qua UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}").
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult StatusCode(int code)
    {
        Response.StatusCode = code;
        if (code == 404)
            return View("NotFound");
        return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
