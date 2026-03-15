using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPage.Application.Features.ComponentLibrary.Commands;
using SPage.Application.Features.ComponentLibrary.Queries;
using System.ComponentModel.DataAnnotations;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
[Route("admin/component-library")]
public sealed class ComponentLibraryController : Controller
{
    private readonly IMediator _mediator;

    public ComponentLibraryController(IMediator mediator) => _mediator = mediator;

    // GET /admin/component-library
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var templates = await _mediator.Send(new GetAllSectionTemplatesQuery());
        return View(templates);
    }

    // GET /admin/component-library/api/list  — JSON endpoint for page builder
    [HttpGet("api/list")]
    public async Task<IActionResult> ApiList()
    {
        var templates = await _mediator.Send(new GetAllSectionTemplatesQuery());
        return Json(templates);
    }

    // GET /admin/component-library/create
    [HttpGet("create")]
    public IActionResult Create() => View("CreateEdit", new SectionTemplateFormModel());

    // POST /admin/component-library/create
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SectionTemplateFormModel model)
    {
        if (!ModelState.IsValid) return View("CreateEdit", model);

        var id = await _mediator.Send(new CreateSectionTemplateCommand(
            model.Name, model.SectionType, model.Description, model.DataJson, model.CssClass));

        // If request is AJAX (from page builder "save to library"), return JSON
        if (Request.Headers.Accept.Any(h => h != null && h.Contains("application/json")))
            return Ok(new { id, success = true });

        TempData["Success"] = "Đã lưu template vào thư viện.";
        return RedirectToAction(nameof(Index));
    }

    // GET /admin/component-library/edit/{id}
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _mediator.Send(new GetSectionTemplateByIdQuery(id));
        if (dto is null) return NotFound();

        var model = new SectionTemplateFormModel
        {
            Id          = dto.Id,
            Name        = dto.Name,
            SectionType = dto.SectionType,
            Description = dto.Description,
            DataJson    = dto.DataJson,
            CssClass    = dto.CssClass
        };
        return View("CreateEdit", model);
    }

    // POST /admin/component-library/edit/{id}
    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SectionTemplateFormModel model)
    {
        if (!ModelState.IsValid) return View("CreateEdit", model);

        await _mediator.Send(new UpdateSectionTemplateCommand(
            id, model.Name, model.SectionType, model.Description, model.DataJson, model.CssClass));

        TempData["Success"] = "Đã cập nhật template.";
        return RedirectToAction(nameof(Index));
    }

    // POST /admin/component-library/delete/{id}
    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteSectionTemplateCommand(id));
        TempData["Success"] = "Đã xóa template.";
        return RedirectToAction(nameof(Index));
    }
}

public sealed class SectionTemplateFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên template không được để trống.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại section.")]
    [MaxLength(100)]
    public string SectionType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // JSON string from the mini section editor
    public string DataJson { get; set; } = "{}";

    [MaxLength(500)]
    public string? CssClass { get; set; }
}
