using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace SPage.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class CacheController : Controller
{
    private readonly IOutputCacheStore _outputCacheStore;

    public CacheController(IOutputCacheStore outputCacheStore)
    {
        _outputCacheStore = outputCacheStore;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Xóa cache";
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearOutput(CancellationToken cancellationToken)
    {
        // Xóa toàn bộ output cache theo tag đã cấu hình trong Program.cs
        var tags = new[] { "posts", "posts-list", "posts-detail", "products", "pages", "sitemap" };

        foreach (var tag in tags)
        {
            await _outputCacheStore.EvictByTagAsync(tag, cancellationToken);
        }

        TempData["Success"] = "Đã xóa cache trang (output cache) cho bài viết, sản phẩm, trang và sitemap.";
        return RedirectToAction(nameof(Index));
    }
}

