using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class SeoController : Controller
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugHistoryService _slugHistory;

    public SeoController(IApplicationDbContext context, ISlugHistoryService slugHistory)
    {
        _context = context;
        _slugHistory = slugHistory;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 30;

        var histories = await _context.SlugHistories
            .AsNoTracking()
            .OrderByDescending(h => h.ChangedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await _context.SlugHistories.CountAsync();

        ViewData["Title"] = "SEO & Redirect";
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        return View(histories);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteHistory(int id)
    {
        var item = await _context.SlugHistories.FindAsync(id);
        if (item is not null)
        {
            await _slugHistory.InvalidateCacheAsync(item.OldSlug);
            _context.SlugHistories.Remove(item);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        TempData["Success"] = "Đã xóa bản ghi redirect.";
        return RedirectToAction(nameof(Index));
    }
}
