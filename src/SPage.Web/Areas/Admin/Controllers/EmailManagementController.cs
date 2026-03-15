using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Email;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class EmailManagementController : Controller
{
    private readonly IApplicationDbContext _context;

    public EmailManagementController(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var config = await _context.EmailSendConfigs.FirstOrDefaultAsync(c => c.Id == 1, HttpContext.RequestAborted);
        var recipients = await _context.RecipientEmails
            .AsNoTracking()
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Email)
            .ToListAsync(HttpContext.RequestAborted);

        ViewData["Title"] = "Quản lý Email";
        ViewBag.Config = config;
        return View(recipients);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveConfig(EmailSendConfigModel model)
    {
        var config = await _context.EmailSendConfigs.FirstOrDefaultAsync(c => c.Id == 1, HttpContext.RequestAborted);
        if (config is null)
        {
            config = new EmailSendConfig { Id = 1 };
            _context.EmailSendConfigs.Add(config);
        }

        config.Host = model.Host?.Trim() ?? "smtp.gmail.com";
        config.Port = model.Port;
        config.EnableSsl = model.EnableSsl;
        config.FromEmail = model.FromEmail?.Trim() ?? "";
        config.FromName = string.IsNullOrWhiteSpace(model.FromName) ? null : model.FromName.Trim();
        config.UserName = string.IsNullOrWhiteSpace(model.UserName) ? null : model.UserName.Trim();
        config.Password = string.IsNullOrWhiteSpace(model.Password) ? config.Password : model.Password.Trim();
        config.IsActive = model.IsActive;
        config.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã lưu cấu hình email gửi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CreateRecipient()
    {
        ViewData["Title"] = "Thêm email nhận";
        return View(new RecipientEmailModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRecipient(RecipientEmailModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError("Email", "Vui lòng nhập email.");
        }
        else if (await _context.RecipientEmails.AnyAsync(r => r.Email == model.Email.Trim(), HttpContext.RequestAborted))
        {
            ModelState.AddModelError("Email", "Email này đã có trong danh sách.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Thêm email nhận";
            return View(model);
        }

        var maxOrder = await _context.RecipientEmails.MaxAsync(r => (int?)r.SortOrder, HttpContext.RequestAborted) ?? 0;
        var entity = new RecipientEmail
        {
            Email = model.Email!.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? null : model.DisplayName.Trim(),
            GroupKey = string.IsNullOrWhiteSpace(model.GroupKey) ? null : model.GroupKey.Trim(),
            IsActive = model.IsActive,
            SortOrder = maxOrder + 1,
            CreatedAt = DateTime.UtcNow
        };
        _context.RecipientEmails.Add(entity);
        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã thêm email nhận.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> EditRecipient(int id)
    {
        var r = await _context.RecipientEmails.FindAsync(id);
        if (r is null) return NotFound();

        ViewData["Title"] = "Sửa email nhận";
        return View(new RecipientEmailModel
        {
            Id = r.Id,
            Email = r.Email,
            DisplayName = r.DisplayName,
            GroupKey = r.GroupKey,
            IsActive = r.IsActive,
            SortOrder = r.SortOrder
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRecipient(int id, RecipientEmailModel model)
    {
        var r = await _context.RecipientEmails.FindAsync(id);
        if (r is null) return NotFound();

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError("Email", "Vui lòng nhập email.");
        }
        else if (await _context.RecipientEmails.AnyAsync(x => x.Email == model.Email.Trim() && x.Id != id, HttpContext.RequestAborted))
        {
            ModelState.AddModelError("Email", "Email này đã có trong danh sách.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Sửa email nhận";
            model.Id = id;
            return View(model);
        }

        r.Email = model.Email!.Trim();
        r.DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? null : model.DisplayName.Trim();
        r.GroupKey = string.IsNullOrWhiteSpace(model.GroupKey) ? null : model.GroupKey.Trim();
        r.IsActive = model.IsActive;
        r.SortOrder = model.SortOrder;
        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã cập nhật email nhận.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRecipient(int id)
    {
        var r = await _context.RecipientEmails.FindAsync(id);
        if (r is null) return NotFound();
        _context.RecipientEmails.Remove(r);
        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        TempData["Success"] = "Đã xóa email nhận.";
        return RedirectToAction(nameof(Index));
    }
}

public sealed class EmailSendConfigModel
{
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class RecipientEmailModel
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? GroupKey { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
