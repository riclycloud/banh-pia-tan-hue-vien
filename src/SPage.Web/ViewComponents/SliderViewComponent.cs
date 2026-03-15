using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Domain.Entities.Content;
using SPage.Infrastructure.Persistence;

namespace SPage.ViewComponents;

public sealed class SliderViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public SliderViewComponent(ApplicationDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync(SliderPosition position)
    {
        var slider = await _db.Sliders
            .Include(s => s.Slides)
            .Where(s => s.IsActive && s.Position == position)
            .FirstOrDefaultAsync();

        return View(slider);
    }
}

