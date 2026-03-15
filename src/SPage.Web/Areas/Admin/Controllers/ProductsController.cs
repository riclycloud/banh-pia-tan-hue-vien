using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Products.Commands;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class ProductsController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public ProductsController(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(int page = 1, string? q = null, int? categoryId = null)
    {
        const int pageSize = 20;
        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.ToLower().Contains(q.ToLower()));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var total = await query.CountAsync();
        var items = await query
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Price,
                p.IsActive,
                p.CreatedAt,
                ThumbnailUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                    ?? p.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()
            })
            .ToListAsync();

        var categories = await _context.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewData["Title"] = "Sản phẩm";
        ViewBag.SearchTerm = q;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = categories
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), categoryId.HasValue && categoryId.Value == c.Id))
            .ToList();
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Thêm sản phẩm";
        await PopulateCategoriesAsync();
        return View(new ProductFormModel
        {
            IsActive = true,
            IsIndexable = true,
            SortOrder = 0
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(model);
        }

        var id = await _mediator.Send(new CreateProductCommand(
            model.Name,
            model.Slug,
            model.Description,
            model.ShortDescription,
            model.Price,
            model.SalePrice,
            model.SKU,
            model.IsActive,
            model.IsNew,
            model.IsFeatured,
            model.IsPromotion,
            model.SortOrder,
            model.CategoryId,
            model.MetaTitle,
            model.MetaDescription,
            model.CanonicalUrl,
            model.IsIndexable,
            model.PrimaryImageUrl,
            model.PrimaryImageAlt));

        TempData["Success"] = "Sản phẩm đã được tạo thành công.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return NotFound();

        var primaryImage = product.Images
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .FirstOrDefault();

        var model = new ProductFormModel
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            Price = product.Price,
            SalePrice = product.SalePrice,
            SKU = product.SKU,
            IsActive = product.IsActive,
            IsNew = product.IsNew,
            IsFeatured = product.IsFeatured,
            IsPromotion = product.IsPromotion,
            SortOrder = product.SortOrder,
            CategoryId = product.CategoryId,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            CanonicalUrl = product.CanonicalUrl,
            IsIndexable = product.IsIndexable,
            PrimaryImageUrl = primaryImage?.ImageUrl,
            PrimaryImageAlt = primaryImage?.AltText
        };

        ViewData["Title"] = "Sửa sản phẩm";
        await PopulateCategoriesAsync(product.CategoryId);
        ViewBag.Gallery = product.Images
            .Where(i => !i.IsPrimary)
            .OrderBy(i => i.SortOrder)
            .ToList();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(model.CategoryId);
            return View(model);
        }

        var success = await _mediator.Send(new UpdateProductCommand(
            model.Id,
            model.Name,
            model.Slug,
            model.Description,
            model.ShortDescription,
            model.Price,
            model.SalePrice,
            model.SKU,
            model.IsActive,
            model.IsNew,
            model.IsFeatured,
            model.IsPromotion,
            model.SortOrder,
            model.CategoryId,
            model.MetaTitle,
            model.MetaDescription,
            model.CanonicalUrl,
            model.IsIndexable,
            model.PrimaryImageUrl,
            model.PrimaryImageAlt));

        if (!success) return NotFound();

        TempData["Success"] = "Sản phẩm đã được cập nhật.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _mediator.Send(new DeleteProductCommand(id));
        var message = success ? "Sản phẩm đã được xóa." : "Không tìm thấy sản phẩm.";
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success, message });
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddImage(int productId, string imageUrl, string? altText)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            TempData["Error"] = "URL hình không hợp lệ.";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product is null) return NotFound();

        var nextSort = product.Images
            .Where(i => !i.IsPrimary)
            .Select(i => (int?)i.SortOrder)
            .Max() ?? 0;

        product.Images.Add(new SPage.Domain.Entities.Product.ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            AltText = altText,
            SortOrder = nextSort + 1,
            IsPrimary = false
        });

        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã thêm hình phụ cho sản phẩm.";
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, int productId)
    {
        var image = await _context.ProductImages
            .FirstOrDefaultAsync(i => i.Id == id && i.ProductId == productId);

        if (image is null) return NotFound();

        if (image.IsPrimary)
        {
            TempData["Error"] = "Không thể xóa ảnh chính tại đây.";
            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã xóa hình phụ.";
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    private async Task PopulateCategoriesAsync(int? selectedId = null)
    {
        var categories = await _context.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Categories = categories
            .Select(c => new SelectListItem(
                c.Name,
                c.Id.ToString(),
                selectedId.HasValue && selectedId.Value == c.Id))
            .ToList();
    }
}

public sealed class ProductFormModel
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(600)]
    public string? Slug { get; set; }

    public string? Description { get; set; }
    public string? ShortDescription { get; set; }

    [System.ComponentModel.DataAnnotations.Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0.")]
    public decimal Price { get; set; }

    [System.ComponentModel.DataAnnotations.Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi không hợp lệ.")]
    public decimal? SalePrice { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string? SKU { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsNew { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public bool IsPromotion { get; set; } = false;
    public int SortOrder { get; set; } = 0;

    [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Chọn chuyên mục sản phẩm.")]
    public int CategoryId { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(70)]
    public string? MetaTitle { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(160)]
    public string? MetaDescription { get; set; }

    public string? CanonicalUrl { get; set; }
    public bool IsIndexable { get; set; } = true;

    public string? PrimaryImageUrl { get; set; }
    public string? PrimaryImageAlt { get; set; }
}

