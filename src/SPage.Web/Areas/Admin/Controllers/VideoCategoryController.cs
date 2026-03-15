using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPage.Application.Features.VideoLibrary.Commands;
using SPage.Application.Features.VideoLibrary.Queries;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class VideoCategoryController : Controller
{
    private readonly IMediator _mediator;
    public VideoCategoryController(IMediator mediator) => _mediator = mediator;

    // GET /admin/video-category
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cats = await _mediator.Send(new GetAllVideoCategoriesQuery());
        return View(cats);
    }

    // POST /admin/video-category/create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, int displayOrder, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Tên danh mục là bắt buộc.";
            return RedirectToAction(nameof(Index));
        }
        await _mediator.Send(new CreateVideoCategoryCommand(name.Trim(), description?.Trim(), displayOrder, isActive));
        TempData["Success"] = $"Đã thêm danh mục \"{name}\".";
        return RedirectToAction(nameof(Index));
    }

    // POST /admin/video-category/edit/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name, string? description, int displayOrder, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Tên danh mục là bắt buộc.";
            return RedirectToAction(nameof(Index));
        }
        await _mediator.Send(new UpdateVideoCategoryCommand(id, name.Trim(), description?.Trim(), displayOrder, isActive));
        TempData["Success"] = "Đã cập nhật danh mục.";
        return RedirectToAction(nameof(Index));
    }

    // POST /admin/video-category/delete/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteVideoCategoryCommand(id));
        TempData["Success"] = "Đã xóa danh mục.";
        return RedirectToAction(nameof(Index));
    }
}
