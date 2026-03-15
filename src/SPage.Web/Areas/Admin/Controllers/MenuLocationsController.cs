using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Navigation;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class MenuLocationsController : Controller
{
    private readonly IApplicationDbContext _context;

    public MenuLocationsController(IApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var locations = await _context.MenuLocations
            .OrderBy(l => l.SortOrder)
            .AsNoTracking()
            .ToListAsync();

        ViewData["Title"] = "Vị trí menu";
        return View(locations);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Thêm vị trí menu";
        return View(new MenuLocation());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuLocation model)
    {
        if (!ModelState.IsValid) return View(model);

        // Auto-generate key from name if empty
        if (string.IsNullOrWhiteSpace(model.Key))
            model.Key = model.Name.ToLowerInvariant().Replace(" ", "-");

        // Check key uniqueness
        if (await _context.MenuLocations.AnyAsync(l => l.Key == model.Key))
        {
            ModelState.AddModelError(nameof(model.Key), "Key này đã tồn tại.");
            return View(model);
        }

        _context.MenuLocations.Add(model);
        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã tạo vị trí menu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var location = await _context.MenuLocations.FindAsync(id);
        if (location is null) return NotFound();

        ViewData["Title"] = "Sửa vị trí menu";
        return View(location);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MenuLocation model)
    {
        if (!ModelState.IsValid) return View(model);

        var existing = await _context.MenuLocations.FindAsync(id);
        if (existing is null) return NotFound();

        // Check key uniqueness (exclude self)
        if (await _context.MenuLocations.AnyAsync(l => l.Key == model.Key && l.Id != id))
        {
            ModelState.AddModelError(nameof(model.Key), "Key này đã tồn tại.");
            return View(model);
        }

        existing.Name = model.Name;
        existing.Key = model.Key;
        existing.SortOrder = model.SortOrder;
        existing.IsActive = model.IsActive;

        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã cập nhật vị trí menu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _context.MenuLocations
            .Include(l => l.MenuItems)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location is null) return NotFound();

        if (location.MenuItems.Count > 0)
        {
            TempData["Error"] = $"Không thể xóa: vị trí \"{location.Name}\" còn {location.MenuItems.Count} menu item.";
            return RedirectToAction(nameof(Index));
        }

        _context.MenuLocations.Remove(location);
        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã xóa vị trí menu.";
        return RedirectToAction(nameof(Index));
    }
}
