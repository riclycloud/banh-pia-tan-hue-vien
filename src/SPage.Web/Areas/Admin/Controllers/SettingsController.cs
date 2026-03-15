using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Seo;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class SettingsController : Controller
{
    private readonly ISiteSettingsService _siteSettings;

    public SettingsController(ISiteSettingsService siteSettings)
    {
        _siteSettings = siteSettings;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var settings = await _siteSettings.GetAsync(cancellationToken);
        ViewData["Title"] = "Cấu hình website";
        return View(settings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SiteSettings model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Cấu hình website";
            return View(model);
        }

        await _siteSettings.UpdateAsync(model, cancellationToken);
        TempData["Success"] = "Đã lưu cấu hình website.";
        return RedirectToAction(nameof(Index));
    }
}

