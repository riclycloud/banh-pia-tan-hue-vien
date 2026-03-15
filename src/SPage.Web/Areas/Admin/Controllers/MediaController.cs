using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Route("[area]/[controller]/[action]")]
[Authorize(Roles = "Admin,ContentManager")]
public sealed class MediaController : Controller
{
    private readonly IWebHostEnvironment _env;

    public MediaController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(IFormFile upload)
    {
        if (upload is null || upload.Length == 0)
        {
            return BadRequest(new { error = new { message = "File không hợp lệ." } });
        }

        var uploadsRootFolder = Path.Combine(_env.WebRootPath, "uploads", "images");
        Directory.CreateDirectory(uploadsRootFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(upload.FileName)}";
        var filePath = Path.Combine(uploadsRootFolder, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await upload.CopyToAsync(stream);
        }

        var url = $"/uploads/images/{fileName}";

        return Json(new 
        { 
            url = url,
            urls = new { @default = url }
        });
    }
}
