using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Navigation;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class MenuController : Controller
{
    private readonly IApplicationDbContext _context;

    public MenuController(IApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? locationId = null)
    {
        var locations = await _context.MenuLocations
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder)
            .AsNoTracking()
            .ToListAsync();

        var query = _context.MenuItems
            .Include(m => m.Location)
            .Include(m => m.Children)
            .AsNoTracking()
            .AsQueryable();

        if (locationId.HasValue)
            query = query.Where(m => m.LocationId == locationId && m.ParentId == null);
        else
            query = query.Where(m => m.ParentId == null);

        var items = await query
            .OrderBy(m => m.LocationId)
            .ThenBy(m => m.SortOrder)
            .ToListAsync();

        ViewData["Title"] = "Menu điều hướng";
        ViewBag.LocationId = locationId;
        ViewBag.Locations = locations;
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder([FromBody] MenuReorderRequest request)
    {
        if (request is null || request.Items is null || request.Items.Count == 0)
            return BadRequest("Dữ liệu không hợp lệ.");

        var ids = request.Items.Select(i => i.Id).ToList();
        var items = await _context.MenuItems
            .Where(m => ids.Contains(m.Id))
            .ToListAsync(HttpContext.RequestAborted);

        foreach (var dto in request.Items)
        {
            var entity = items.FirstOrDefault(m => m.Id == dto.Id);
            if (entity is null) continue;
            entity.ParentId = dto.ParentId;
            entity.SortOrder = dto.SortOrder;
        }

        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok(new { success = true });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        ViewData["Title"] = id is null ? "Thêm menu" : "Sửa menu";

        await PopulateViewBagAsync(null);

        if (id is null)
            return View(new MenuItem());

        var item = await _context.MenuItems.FindAsync(id.Value);
        if (item is null) return NotFound();

        await PopulateViewBagAsync(item.LocationId);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, MenuItem model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateViewBagAsync(model.LocationId);
            return View(model);
        }

        if (id is null || id == 0)
        {
            _context.MenuItems.Add(model);
            TempData["Success"] = "Đã tạo menu mới.";
        }
        else
        {
            var existing = await _context.MenuItems.FindAsync(id.Value);
            if (existing is null) return NotFound();

            existing.Title = model.Title;
            existing.Url = model.Url;
            existing.LocationId = model.LocationId;
            existing.ParentId = model.ParentId;
            existing.SortOrder = model.SortOrder;
            existing.IsActive = model.IsActive;
            existing.OpenInNewTab = model.OpenInNewTab;

            TempData["Success"] = "Đã cập nhật menu.";
        }

        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        return RedirectToAction(nameof(Index), new { locationId = model.LocationId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item is not null)
        {
            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync(HttpContext.RequestAborted);
            TempData["Success"] = "Đã xóa menu.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateViewBagAsync(int? currentLocationId)
    {
        var locations = await _context.MenuLocations
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.Locations = new SelectList(locations, "Id", "Name", currentLocationId);

        var parents = await _context.MenuItems
            .Include(m => m.Location)
            .Where(m => m.ParentId == null)
            .OrderBy(m => m.Title)
            .AsNoTracking()
            .ToListAsync();
        ViewBag.Parents = parents;
    }
}

public sealed class MenuReorderRequest
{
    public List<MenuReorderItem> Items { get; set; } = [];
}

public sealed class MenuReorderItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
}
