using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Domain.Entities.Navigation;
using SPage.Infrastructure.Persistence;

namespace SPage.ViewComponents;

public sealed class MenuViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public MenuViewComponent(ApplicationDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync(string key, string? view = null)
    {
        var items = await _db.MenuItems
            .Include(x => x.Children.Where(c => c.IsActive))
            .Where(x => x.IsActive && x.Location.Key == key && x.Location.IsActive && x.ParentId == null)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        // Use named view if provided, otherwise fall back to key-named or Default
        var viewName = view ?? key;
        return View(viewName, items);
    }
}
