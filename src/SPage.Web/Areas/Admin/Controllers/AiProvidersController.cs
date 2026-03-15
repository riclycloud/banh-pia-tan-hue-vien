using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Ai;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class AiProvidersController : Controller
{
    private readonly IApplicationDbContext _context;

    public AiProvidersController(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _context.AiProviderConfigs
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.ProviderKey)
            .ToListAsync(HttpContext.RequestAborted);
        ViewData["Title"] = "API Key AI";
        return View(list);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Thêm API Key AI";
        return View(new AiProviderConfigModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AiProviderConfigModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ProviderKey))
        {
            ModelState.AddModelError("ProviderKey", "Chọn hoặc nhập mã provider.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Thêm API Key AI";
            return View(model);
        }

        var maxOrder = await _context.AiProviderConfigs.MaxAsync(c => (int?)c.SortOrder, HttpContext.RequestAborted) ?? 0;
        var entity = new AiProviderConfig
        {
            ProviderKey = model.ProviderKey!.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? null : model.DisplayName.Trim(),
            ApiKey = string.IsNullOrWhiteSpace(model.ApiKey) ? null : model.ApiKey.Trim(),
            BaseUrl = string.IsNullOrWhiteSpace(model.BaseUrl) ? null : model.BaseUrl.Trim(),
            ModelName = string.IsNullOrWhiteSpace(model.ModelName) ? null : model.ModelName.Trim(),
            IsActive = model.IsActive,
            SortOrder = maxOrder + 1,
            UpdatedAt = DateTime.UtcNow
        };
        _context.AiProviderConfigs.Add(entity);
        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã thêm cấu hình AI.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var c = await _context.AiProviderConfigs.FindAsync(id);
        if (c is null) return NotFound();
        ViewData["Title"] = "Sửa API Key AI";
        return View(new AiProviderConfigModel
        {
            Id = c.Id,
            ProviderKey = c.ProviderKey,
            DisplayName = c.DisplayName,
            ApiKey = c.ApiKey,
            BaseUrl = c.BaseUrl,
            ModelName = c.ModelName,
            IsActive = c.IsActive,
            SortOrder = c.SortOrder
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AiProviderConfigModel model)
    {
        var c = await _context.AiProviderConfigs.FindAsync(id);
        if (c is null) return NotFound();
        if (string.IsNullOrWhiteSpace(model.ProviderKey))
        {
            ModelState.AddModelError("ProviderKey", "Chọn hoặc nhập mã provider.");
        }
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Sửa API Key AI";
            model.Id = id;
            return View(model);
        }
        c.ProviderKey = model.ProviderKey!.Trim();
        c.DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? null : model.DisplayName.Trim();
        c.ApiKey = string.IsNullOrWhiteSpace(model.ApiKey) ? c.ApiKey : model.ApiKey.Trim();
        c.BaseUrl = string.IsNullOrWhiteSpace(model.BaseUrl) ? null : model.BaseUrl.Trim();
        c.ModelName = string.IsNullOrWhiteSpace(model.ModelName) ? null : model.ModelName.Trim();
        c.IsActive = model.IsActive;
        c.SortOrder = model.SortOrder;
        c.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã cập nhật cấu hình AI.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _context.AiProviderConfigs.FindAsync(id);
        if (c is null) return NotFound();
        _context.AiProviderConfigs.Remove(c);
        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã xóa cấu hình AI.";
        return RedirectToAction(nameof(Index));
    }
}

public sealed class AiProviderConfigModel
{
    public int Id { get; set; }
    public string? ProviderKey { get; set; }
    public string? DisplayName { get; set; }
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public string? ModelName { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
