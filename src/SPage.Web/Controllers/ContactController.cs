using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Contact;
using SPage.ViewModels.Seo;

namespace SPage.Controllers;

public sealed class ContactController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly ISiteSettingsService _siteSettings;
    private readonly IEmailService _emailService;

    public ContactController(
        IApplicationDbContext db,
        ISiteSettingsService siteSettings,
        IEmailService emailService)
    {
        _db = db;
        _siteSettings = siteSettings;
        _emailService = emailService;
    }

    [HttpGet]
    [Route("lien-he")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var fields   = await _db.ContactFields
            .AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.SortOrder)
            .ToListAsync(cancellationToken);

        var settings = await _siteSettings.GetAsync(cancellationToken);
        ViewData["Seo"] = new SeoViewModel
        {
            Title           = $"Liên hệ — {settings.SiteName}",
            MetaDescription = settings.DefaultMetaDescription
                ?? $"Liên hệ với {settings.SiteName}. Chúng tôi sẽ phản hồi trong thời gian sớm nhất.",
            CanonicalUrl    = Url.Action("Index", "Contact", null, Request.Scheme),
            IsIndexable     = true,
            OgType          = "website"
        };

        return View(fields);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Route("lien-he")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken, Dictionary<string, string> formValues)
    {
        var fields = await _db.ContactFields
            .AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.SortOrder)
            .ToListAsync(cancellationToken);

        var errors = new List<string>();

        foreach (var field in fields)
        {
            formValues.TryGetValue(field.Name, out var value);
            if (field.IsRequired && string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Vui lòng nhập \"{field.Label}\".");
            }
        }

        if (errors.Count > 0)
        {
            ViewBag.Errors = errors;
            return View(fields);
        }

        var settings = await _siteSettings.GetAsync(cancellationToken);
        var toEmail = settings.ContactEmail ?? settings.Email;
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            TempData["Error"] = "Hệ thống chưa cấu hình email nhận liên hệ.";
            return View(fields);
        }

        var subject = $"Liên hệ mới từ {settings.SiteName}";

        var body = new System.Text.StringBuilder();
        body.AppendLine("<h2>Thông tin liên hệ mới</h2>");
        body.AppendLine("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\" style=\"border-collapse:collapse;\">");
        body.AppendLine("<tbody>");

        foreach (var field in fields)
        {
            formValues.TryGetValue(field.Name, out var value);
            body.AppendLine($"<tr><th align=\"left\">{System.Net.WebUtility.HtmlEncode(field.Label)}</th><td>{System.Net.WebUtility.HtmlEncode(value ?? string.Empty)}</td></tr>");
        }

        body.AppendLine("</tbody></table>");
        var now = DateTimeOffset.Now;
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        body.AppendLine($"<p>Thời gian gửi: {now:dd/MM/yyyy HH:mm}</p>");
        body.AppendLine($"<p>IP: {ip}</p>");

        await _emailService.SendEmailAsync(toEmail, subject, body.ToString(), cancellationToken);

        // Lưu vào DB để quản lý hộp thư
        formValues.TryGetValue("subject", out var formSubject);
        var message = new ContactMessage
        {
            CreatedAt = now,
            Subject   = !string.IsNullOrWhiteSpace(formSubject) ? formSubject.Trim() : "Liên hệ chung",
            SenderIp   = ip,
            Status    = ContactMessageStatus.New,
            DataJson  = System.Text.Json.JsonSerializer.Serialize(formValues)
        };

        // Cố gắng map một số field phổ biến
        if (formValues.TryGetValue("fullName", out var fullName))
            message.SenderName = fullName;
        if (formValues.TryGetValue("email", out var emailVal))
            message.SenderEmail = emailVal;
        if (formValues.TryGetValue("phone", out var phoneVal))
            message.SenderPhone = phoneVal;

        _db.ContactMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        TempData["Success"] = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi trong thời gian sớm nhất.";
        return RedirectToAction(nameof(Index));
    }
}

