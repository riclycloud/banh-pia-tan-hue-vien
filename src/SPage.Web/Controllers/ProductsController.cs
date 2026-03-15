using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SPage.Application.Features.Products.DTOs;
using SPage.Application.Features.Products.Queries;
using SPage.ViewModels.Seo;

namespace SPage.Controllers;

public sealed class ProductsController : Controller
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    [OutputCache(PolicyName = "ProductsList")]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        [FromQuery] int? category = null,
        [FromQuery] string? categorySlug = null,
        [FromQuery] string? q = null,
        [FromQuery] string? sort = "newest",
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetProductsQuery(Page: page, PageSize: 12, CategoryId: category, CategorySlug: categorySlug, SearchTerm: q, SortBy: sort), cancellationToken);

        ViewData["Seo"] = new SeoViewModel
        {
            Title = "Sản phẩm",
            MetaDescription = "Danh sách sản phẩm.",
            CanonicalUrl = Url.Action("Index", "Products", new { page }, Request.Scheme),
            IsIndexable = page == 1 && string.IsNullOrEmpty(q)
        };

        return View(result);
    }

    [OutputCache(PolicyName = "PostDetail")]
    public async Task<IActionResult> Detail(string categorySlug, string productSlug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productSlug)) return NotFound();

        var product = await _mediator.Send(new GetProductBySlugQuery(productSlug), cancellationToken);
        if (product is null) return NotFound();

        // Canonical: chuyển hướng nếu categorySlug không khớp (URL đúng là /{categorySlug}/{productSlug})
        var expectedCategorySlug = product.CategorySlug ?? "san-pham";
        if (!string.IsNullOrEmpty(categorySlug) && !string.Equals(categorySlug, expectedCategorySlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToRoutePermanent("product-detail", new { categorySlug = expectedCategorySlug, productSlug = product.Slug });
        }

        var canonicalUrl = product.CanonicalUrl
            ?? Url.RouteUrl("product-detail", new { categorySlug = expectedCategorySlug, productSlug = product.Slug }, Request.Scheme);

        ViewData["Seo"] = new SeoViewModel
        {
            Title = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            CanonicalUrl = canonicalUrl,
            IsIndexable = product.IsIndexable
        };

        return View(product);
    }
}
