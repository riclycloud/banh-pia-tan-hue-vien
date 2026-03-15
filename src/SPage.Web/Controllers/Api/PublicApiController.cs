using MediatR;
using Microsoft.AspNetCore.Mvc;
using SPage.Application.Features.Categories.Queries;
using SPage.Application.Features.Contact.Commands;
using SPage.Application.Features.Contact.Queries;
using SPage.Application.Features.Menus.Queries;
using SPage.Application.Features.Posts.Queries;
using SPage.Application.Features.ProductCategories.Queries;
using SPage.Application.Features.Products.Queries;
using SPage.Application.Features.Sliders.Queries;
using SPage.Domain.Entities.Content;

namespace SPage.Web.Controllers.Api;

/// <summary>
/// Public REST API — gọi từ JavaScript trong các section trang động.
/// Tất cả endpoints đều public (không yêu cầu auth).
/// Base: /api/
/// </summary>
[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class PublicApiController : ControllerBase
{
    private readonly ISender _mediator;

    public PublicApiController(ISender mediator) => _mediator = mediator;

    // ───────────────────────────────────────────────
    // POSTS
    // ───────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách bài viết (có phân trang, lọc theo danh mục/tag/từ khóa).
    /// GET /api/posts?categorySlug=tin-tuc&amp;page=1&amp;pageSize=10&amp;search=
    /// </summary>
    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts(
        [FromQuery] string? categorySlug = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? tagSlug = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page     = Math.Max(1, page);

        var result = await _mediator.Send(new GetPostsQuery(
            Page:         page,
            PageSize:     pageSize,
            CategoryId:   categoryId,
            CategorySlug: categorySlug,
            TagSlug:      tagSlug,
            SearchTerm:   search
        ), ct);

        return Ok(result);
    }

    // ───────────────────────────────────────────────
    // POST CATEGORIES
    // ───────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách danh mục bài viết.
    /// GET /api/post-categories
    /// </summary>
    [HttpGet("post-categories")]
    public async Task<IActionResult> GetPostCategories(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), ct);
        return Ok(result);
    }

    // ───────────────────────────────────────────────
    // PRODUCTS
    // ───────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách sản phẩm (có phân trang, lọc theo danh mục/tìm kiếm/sắp xếp).
    /// GET /api/products?categorySlug=banh-ngot&amp;page=1&amp;pageSize=12&amp;sort=newest&amp;search=
    /// sort: newest | price_asc | price_desc | name_asc
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? categorySlug = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] string sort = "newest",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page     = Math.Max(1, page);

        var result = await _mediator.Send(new GetProductsQuery(
            Page:         page,
            PageSize:     pageSize,
            CategoryId:   categoryId,
            CategorySlug: categorySlug,
            SearchTerm:   search,
            SortBy:       sort
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm khuyến mãi (IsPromotion = true).
    /// GET /api/products/on-sale?page=1&amp;pageSize=12
    /// </summary>
    [HttpGet("products/on-sale")]
    public async Task<IActionResult> GetOnSaleProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page     = Math.Max(1, page);

        var result = await _mediator.Send(new GetProductsQuery(
            Page:        page,
            PageSize:    pageSize,
            IsPromotion: true
        ), ct);

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm mới (IsNew = true).
    /// GET /api/products/new?page=1&amp;pageSize=12
    /// </summary>
    [HttpGet("products/new")]
    public async Task<IActionResult> GetNewProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page     = Math.Max(1, page);

        var result = await _mediator.Send(new GetProductsQuery(
            Page:     page,
            PageSize: pageSize,
            IsNew:    true
        ), ct);

        return Ok(result);
    }

    // ───────────────────────────────────────────────
    // PRODUCT CATEGORIES
    // ───────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách danh mục sản phẩm.
    /// GET /api/product-categories
    /// </summary>
    [HttpGet("product-categories")]
    public async Task<IActionResult> GetProductCategories(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetProductCategoriesQuery(), ct);
        return Ok(result);
    }

    // ───────────────────────────────────────────────
    // CONTACT
    // ───────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách trường form liên hệ (để render form động).
    /// GET /api/contact-fields
    /// </summary>
    [HttpGet("contact-fields")]
    public async Task<IActionResult> GetContactFields(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetContactFieldsQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Gửi form liên hệ.
    /// POST /api/contact
    /// Body: { "subject": "Đăng ký tham quan", "fields": { "name": "...", "email": "...", "message": "..." } }
    /// </summary>
    [HttpPost("contact")]
    public async Task<IActionResult> SubmitContact(
        [FromBody] ContactSubmitRequest body,
        CancellationToken ct = default)
    {
        if (body?.Fields is null || body.Fields.Count == 0)
            return BadRequest(new { success = false, message = "Vui lòng điền thông tin liên hệ." });

        var senderIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        await _mediator.Send(new SubmitContactCommand(body.Fields, body.Subject, senderIp), ct);

        return Ok(new { success = true, message = "Cảm ơn! Chúng tôi sẽ liên hệ lại sớm nhất." });
    }

    // ───────────────────────────────────────────────
    // MENUS
    // ───────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách menu theo vị trí (location key).
    /// GET /api/menus/header
    /// GET /api/menus/footer
    /// </summary>
    [HttpGet("menus/{location}")]
    public async Task<IActionResult> GetMenu(string location, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(location))
            return BadRequest(new { message = "Vị trí menu không hợp lệ." });

        var result = await _mediator.Send(new GetMenuByLocationQuery(location.ToLower()), ct);
        return Ok(result);
    }

    // ───────────────────────────────────────────────
    // SLIDERS
    // ───────────────────────────────────────────────

    /// <summary>
    /// Lấy danh sách slide theo vị trí banner.
    /// GET /api/sliders/home-hero
    /// GET /api/sliders/home-below-hero
    /// GET /api/sliders/sidebar-right
    /// GET /api/sliders/footer-banner
    /// Hoặc dùng số: /api/sliders/0  (0=HomeHero, 1=HomeBelowHero, 2=SidebarRight, 3=FooterBanner)
    /// </summary>
    [HttpGet("sliders/{position}")]
    public async Task<IActionResult> GetSlider(string position, CancellationToken ct = default)
    {
        var pos = ParseSliderPosition(position);
        if (pos is null)
            return BadRequest(new
            {
                message = "Vị trí không hợp lệ. Dùng: home-hero | home-below-hero | sidebar-right | footer-banner (hoặc số 0–3)."
            });

        var result = await _mediator.Send(new GetSliderByPositionQuery(pos.Value), ct);
        return Ok(result);
    }

    // ───────────────────────────────────────────────
    // HELPERS
    // ───────────────────────────────────────────────

    private static SliderPosition? ParseSliderPosition(string input)
    {
        // Thử parse bằng số nguyên trước
        if (int.TryParse(input, out var intVal) && Enum.IsDefined(typeof(SliderPosition), intVal))
            return (SliderPosition)intVal;

        // Thử parse bằng tên (kebab-case → PascalCase)
        var normalized = input.ToLower() switch
        {
            "home-hero"        => SliderPosition.HomeHero,
            "home-below-hero"  => SliderPosition.HomeBelowHero,
            "sidebar-right"    => SliderPosition.SidebarRight,
            "footer-banner"    => SliderPosition.FooterBanner,
            _                  => (SliderPosition?)null
        };
        return normalized;
    }
}

/// <summary>Request body cho POST /api/contact</summary>
public sealed class ContactSubmitRequest
{
    /// <summary>Tiêu đề / loại form để phân biệt (vd: Đăng ký tham quan Nhà máy, Liên hệ chung).</summary>
    public string? Subject { get; set; }

    /// <summary>Dữ liệu form: key = tên trường (name/email/phone/message...), value = giá trị nhập.</summary>
    public Dictionary<string, string> Fields { get; set; } = [];
}
