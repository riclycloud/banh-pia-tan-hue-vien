using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Contact;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class ContactMessagesController : Controller
{
    private readonly IApplicationDbContext _context;

    public ContactMessagesController(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(ContactMessageStatus? status = null)
    {
        var query = _context.ContactMessages
            .AsNoTracking()
            .OrderByDescending(m => m.CreatedAt)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }

        var items = await query.ToListAsync();

        ViewData["Title"] = "Hộp thư liên hệ";
        ViewBag.Status = status;
        return View(items);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var message = await _context.ContactMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message is null) return NotFound();

        ViewData["Title"] = "Chi tiết liên hệ";
        return View(message);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ContactMessageStatus status, string? notes)
    {
        var message = await _context.ContactMessages.FirstOrDefaultAsync(m => m.Id == id);
        if (message is null) return NotFound();

        message.Status = status;
        message.Notes = notes;

        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã cập nhật trạng thái liên hệ.";
        return RedirectToAction(nameof(Detail), new { id });
    }
}

