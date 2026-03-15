using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SPage.Application.Features.ProductCategories.Commands;
using SPage.Application.Features.ProductCategories.Queries;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class ProductCategoriesController : Controller
{
    private readonly IMediator _mediator;

    public ProductCategoriesController(IMediator mediator) => _mediator = mediator;

    public async Task<IActionResult> Index()
    {
        var categories = await _mediator.Send(new GetProductCategoriesQuery());
        ViewData["Title"] = "Chuyên mục sản phẩm";
        return View(categories);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, int? parentId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Tên chuyên mục không được để trống.";
            return RedirectToAction(nameof(Index));
        }

        await _mediator.Send(new CreateProductCategoryCommand(name, description, parentId));
        TempData["Success"] = "Chuyên mục sản phẩm đã được thêm.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _mediator.Send(new DeleteProductCategoryCommand(id));
        var message = success ? "Chuyên mục đã được xóa." : "Không thể xóa chuyên mục này.";
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success, message });
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _mediator.Send(new GetProductCategoryByIdQuery(id));
        if (category is null) return NotFound();

        var categories = await _mediator.Send(new GetProductCategoriesQuery());
        ViewBag.ParentCategories = categories
            .Where(c => c.Id != id)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == category.ParentId))
            .ToList();

        var model = new ProductCategoryEditModel
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            ParentId = category.ParentId,
            MetaTitle = category.MetaTitle,
            MetaDescription = category.MetaDescription,
            CanonicalUrl = category.CanonicalUrl,
            IsIndexable = category.IsIndexable,
            ThumbnailUrl = category.ThumbnailUrl,
            ThumbnailAlt = category.ThumbnailAlt
        };

        ViewData["Title"] = "Sửa chuyên mục sản phẩm";
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductCategoryEditModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            var categories = await _mediator.Send(new GetProductCategoriesQuery());
            ViewBag.ParentCategories = categories
                .Where(c => c.Id != id)
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.ParentId))
                .ToList();
            return View(model);
        }

        var success = await _mediator.Send(new UpdateProductCategoryCommand(
            model.Id,
            model.Name,
            model.Slug,
            model.Description,
            model.ParentId,
            model.MetaTitle,
            model.MetaDescription,
            model.CanonicalUrl,
            model.IsIndexable,
            model.ThumbnailUrl,
            model.ThumbnailAlt));

        if (!success) return NotFound();

        TempData["Success"] = "Chuyên mục đã được cập nhật.";
        return RedirectToAction(nameof(Edit), new { id });
    }
}

public sealed class ProductCategoryEditModel
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Tên chuyên mục không được để trống.")]
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? ParentId { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(70)]
    public string? MetaTitle { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(160)]
    public string? MetaDescription { get; set; }

    public string? CanonicalUrl { get; set; }
    public bool IsIndexable { get; set; } = true;

    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string? Slug { get; set; }

    public string? ThumbnailUrl { get; set; }
    public string? ThumbnailAlt { get; set; }
}
