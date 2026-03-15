using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPage.Application.Features.MediaLibrary.Commands;
using SPage.Application.Features.MediaLibrary.Queries;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,ContentManager")]
[Route("admin/media-library")]
public sealed class MediaLibraryController : Controller
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

    private const long MaxBytes = 10 * 1024 * 1024; // 10 MB per file

    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public MediaLibraryController(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _env      = env;
    }

    // GET /admin/media-library
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var files = await _mediator.Send(new GetAllMediaFilesQuery());
        return View(files);
    }

    // GET /admin/media-library/api/list  — JSON for image picker
    [HttpGet("api/list")]
    public async Task<IActionResult> ApiList()
    {
        var files = await _mediator.Send(new GetAllMediaFilesQuery());
        return Json(files.Select(f => new
        {
            f.Id,
            f.FileName,
            f.Url,
            f.FileSize,
            f.MimeType,
            f.AltText,
            CreatedAt = f.CreatedAt.ToString("dd/MM/yyyy HH:mm")
        }));
    }

    // POST /admin/media-library/upload  — multi-file upload, returns JSON array
    [HttpPost("upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(List<IFormFile> files)
    {
        if (files is null || files.Count == 0)
            return BadRequest(new { error = "Không có file nào được chọn." });

        var results = new List<object>();
        var errors  = new List<string>();

        var folder = Path.Combine(_env.WebRootPath, "uploads", "images");
        Directory.CreateDirectory(folder);

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            if (file.Length > MaxBytes)
            {
                errors.Add($"{file.FileName}: File quá lớn (tối đa 10 MB).");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext))
            {
                errors.Add($"{file.FileName}: Định dạng không hỗ trợ.");
                continue;
            }

            var storedName = $"{Guid.NewGuid():N}{ext}";
            var filePath   = Path.Combine(folder, storedName);
            var mimeType   = MimeMap.GetValueOrDefault(ext, "application/octet-stream");

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/images/{storedName}";

            var id = await _mediator.Send(new CreateMediaFileCommand(
                FileName:   file.FileName,
                StoredName: storedName,
                Url:        url,
                FileSize:   file.Length,
                MimeType:   mimeType));

            results.Add(new { id, url, fileName = file.FileName, fileSize = file.Length });
        }

        return Ok(new { results, errors });
    }

    // POST /admin/media-library/delete/{id}
    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteMediaFileCommand(id));
        return Ok(new { success = true });
    }

    // POST /admin/media-library/update-alt/{id}
    [HttpPost("update-alt/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAlt(int id, [FromBody] UpdateAltRequest req)
    {
        await _mediator.Send(new UpdateMediaFileAltCommand(id, req.AltText));
        return Ok(new { success = true });
    }

    public sealed class UpdateAltRequest
    {
        public string? AltText { get; set; }
    }
}
