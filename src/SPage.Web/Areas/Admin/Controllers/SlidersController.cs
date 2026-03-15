using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Content;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class SlidersController : Controller
{
    private readonly IApplicationDbContext _context;

    public SlidersController(IApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var sliders = await _context.Sliders
            .Include(s => s.Slides)
            .AsNoTracking()
            .OrderBy(s => s.Position)
            .ThenBy(s => s.Name)
            .ToListAsync();

        ViewData["Title"] = "Slider & Banner";
        return View(sliders);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Thêm slider";
        return View("Edit", new Slider());
    }

    public async Task<IActionResult> Edit(int id)
    {
        var slider = await _context.Sliders
            .Include(s => s.Slides)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (slider is null) return NotFound();

        ViewData["Title"] = "Sửa slider";
        return View(slider);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Slider model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Id == 0)
        {
            _context.Sliders.Add(model);
            TempData["Success"] = "Đã tạo slider.";
        }
        else
        {
            var existing = await _context.Sliders.FindAsync(model.Id);
            if (existing is null) return NotFound();

            existing.Name = model.Name;
            existing.Position = model.Position;
            existing.IsActive = model.IsActive;

            TempData["Success"] = "Đã cập nhật slider.";
        }

        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var slider = await _context.Sliders.FindAsync(id);
        if (slider is not null)
        {
            _context.Sliders.Remove(slider);
            await _context.SaveChangesAsync(HttpContext.RequestAborted);
            TempData["Success"] = "Đã xóa slider.";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> EditSlides(int id)
    {
        var slider = await _context.Sliders
            .Include(s => s.Slides)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (slider is null) return NotFound();

        ViewData["Title"] = $"Slide cho: {slider.Name}";
        return View(slider);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSlides(int id, List<Slide> slides, List<IFormFile>? slideImages)
    {
        var slider = await _context.Sliders
            .Include(s => s.Slides)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (slider is null) return NotFound();

        // Ánh xạ file upload vào ImageUrl
        if (slideImages is not null && slideImages.Count > 0)
        {
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "sliders");
            Directory.CreateDirectory(uploadsRoot);

            for (var i = 0; i < slides.Count; i++)
            {
                if (slideImages.Count <= i) break;
                var file = slideImages[i];
                if (file is null || file.Length == 0) continue;

                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsRoot, fileName);

                await using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream, HttpContext.RequestAborted);
                }

                slides[i].ImageUrl = $"/uploads/sliders/{fileName}";
            }
        }

        // Xóa slide cũ
        _context.Slides.RemoveRange(slider.Slides);

        // Thêm lại từ form
        var validSlides = slides
            .Where(s => !string.IsNullOrWhiteSpace(s.ImageUrl))
            .ToList();

        foreach (var s in validSlides)
        {
            s.Id = 0;
            s.SliderId = id;
        }

        _context.Slides.AddRange(validSlides);
        await _context.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["Success"] = "Đã cập nhật slide.";
        return RedirectToAction(nameof(EditSlides), new { id });
    }
}

