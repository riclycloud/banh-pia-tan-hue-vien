using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Store;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class StoreLocationsController : Controller
{
    private readonly IApplicationDbContext _context;

    public StoreLocationsController(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(StoreRegionType? region = null)
    {
        var query = _context.StoreLocations
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .AsQueryable();

        if (region.HasValue)
        {
            query = query.Where(x => x.RegionType == region.Value);
        }

        var items = await query.ToListAsync();

        ViewData["Title"] = "Địa chỉ cửa hàng";
        ViewBag.Region = region;
        return View(items);
    }

    [HttpGet]
    public IActionResult Edit(int? id)
    {
        ViewData["Title"] = id is null ? "Thêm địa chỉ cửa hàng" : "Sửa địa chỉ cửa hàng";

        if (id is null)
        {
            return View(new StoreLocation { IsActive = true });
        }

        var location = _context.StoreLocations.FirstOrDefault(x => x.Id == id.Value);
        if (location is null) return NotFound();

        return View(location);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, StoreLocation model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = id is null ? "Thêm địa chỉ cửa hàng" : "Sửa địa chỉ cửa hàng";
            return View(model);
        }

        if (id is null || id == 0)
        {
            _context.StoreLocations.Add(model);
            TempData["Success"] = "Đã thêm địa chỉ cửa hàng.";
        }
        else
        {
            var existing = await _context.StoreLocations.FirstOrDefaultAsync(x => x.Id == id.Value);
            if (existing is null) return NotFound();

            existing.Name = model.Name;
            existing.FullAddress = model.FullAddress;
            existing.ProvinceOrCity = model.ProvinceOrCity;
            existing.Country = model.Country;
            existing.Latitude = model.Latitude;
            existing.Longitude = model.Longitude;
            existing.MapLink = model.MapLink;
            existing.LandlinePhone = model.LandlinePhone;
            existing.MobilePhone = model.MobilePhone;
            existing.ManagerName = model.ManagerName;
            existing.LocationType = model.LocationType;
            existing.RegionType = model.RegionType;
            existing.IsActive = model.IsActive;
            existing.SortOrder = model.SortOrder;

            TempData["Success"] = "Đã cập nhật địa chỉ cửa hàng.";
        }

        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _context.StoreLocations.FirstOrDefaultAsync(x => x.Id == id);
        if (location is not null)
        {
            _context.StoreLocations.Remove(location);
            await _context.SaveChangesAsync(HttpContext.RequestAborted);
            TempData["Success"] = "Đã xóa địa chỉ cửa hàng.";
        }

        return RedirectToAction(nameof(Index));
    }
}

