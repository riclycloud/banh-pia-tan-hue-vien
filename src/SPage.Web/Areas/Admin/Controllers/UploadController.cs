using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPage.Application.Features.MediaLibrary.Commands;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class UploadController : Controller
{
    private static readonly string[] AllowedExt =
        [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"];

    private static readonly Dictionary<string, string> MimeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"]  = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"]  = "image/png",
        [".gif"]  = "image/gif",
        [".webp"] = "image/webp",
        [".svg"]  = "image/svg+xml"
    };

    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _env;
    private readonly IMediator _mediator;

    public UploadController(IWebHostEnvironment env, IMediator mediator)
    {
        _env      = env;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Image(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Không có file." });

        if (file.Length > MaxBytes)
            return BadRequest(new { error = "File quá lớn (tối đa 5 MB)." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExt.Contains(ext))
            return BadRequest(new { error = "Định dạng file không được hỗ trợ." });

        var folder = Path.Combine(_env.WebRootPath, "uploads", "images");
        Directory.CreateDirectory(folder);

        var storedName = $"{Guid.NewGuid():N}{ext}";
        var filePath   = Path.Combine(folder, storedName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var url      = $"/uploads/images/{storedName}";
        var mimeType = MimeMap.GetValueOrDefault(ext, "application/octet-stream");

        // Track upload in media library DB
        var id = await _mediator.Send(new CreateMediaFileCommand(
            FileName:   file.FileName,
            StoredName: storedName,
            Url:        url,
            FileSize:   file.Length,
            MimeType:   mimeType));

        return Ok(new { url, id });
    }
}
