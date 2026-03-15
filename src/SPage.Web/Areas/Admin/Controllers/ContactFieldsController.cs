using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Contact;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class ContactFieldsController : Controller
{
    private readonly IApplicationDbContext _context;

    public ContactFieldsController(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var fields = await _context.ContactFields
            .AsNoTracking()
            .OrderBy(f => f.SortOrder)
            .ToListAsync();

        ViewData["Title"] = "Trường form liên hệ";
        return View(fields);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        ViewData["Title"] = id is null ? "Thêm trường liên hệ" : "Sửa trường liên hệ";

        if (id is null)
        {
            return View(new ContactField { IsRequired = true, IsActive = true });
        }

        var field = await _context.ContactFields.FindAsync(id.Value);
        if (field is null) return NotFound();

        return View(field);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, ContactField model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = id is null ? "Thêm trường liên hệ" : "Sửa trường liên hệ";
            return View(model);
        }

        model.Name = model.Name.Trim();

        if (id is null || id == 0)
        {
            _context.ContactFields.Add(model);
            TempData["Success"] = "Đã tạo trường liên hệ.";
        }
        else
        {
            var existing = await _context.ContactFields.FindAsync(id.Value);
            if (existing is null) return NotFound();

            existing.Name = model.Name;
            existing.Label = model.Label;
            existing.Placeholder = model.Placeholder;
            existing.FieldType = model.FieldType;
            existing.IsRequired = model.IsRequired;
            existing.IsActive = model.IsActive;
            existing.SortOrder = model.SortOrder;

            TempData["Success"] = "Đã cập nhật trường liên hệ.";
        }

        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var field = await _context.ContactFields.FindAsync(id);
        if (field is not null)
        {
            _context.ContactFields.Remove(field);
            await _context.SaveChangesAsync(HttpContext.RequestAborted);
            TempData["Success"] = "Đã xóa trường liên hệ.";
        }

        return RedirectToAction(nameof(Index));
    }
}

