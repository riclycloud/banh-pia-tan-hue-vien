using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SPage.Application.Common.Interfaces;

namespace SPage.Infrastructure.Services;

public sealed class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private static readonly int[] BreakPoints = [400, 800, 1200];

    public ImageService(IWebHostEnvironment env) => _env = env;

    public async Task<ImageUploadResult> ProcessAndSaveAsync(
        IFormFile file,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var fileName = Guid.NewGuid().ToString("N");
        var uploadDir = Path.Combine(_env.WebRootPath, "images", folder);
        Directory.CreateDirectory(uploadDir);

        var result = new ImageUploadResult();

        using var image = await Image.LoadAsync(file.OpenReadStream(), cancellationToken);

        // ★ Lưu ảnh gốc dạng WebP (quality 82)
        var originalPath = Path.Combine(uploadDir, $"{fileName}.webp");
        await image.SaveAsWebpAsync(originalPath,
            new WebpEncoder { Quality = 82 }, cancellationToken);
        result.OriginalUrl = $"/images/{folder}/{fileName}.webp";

        // ★ Tạo responsive variants cho srcset
        foreach (var width in BreakPoints)
        {
            if (image.Width <= width) continue;

            using var resized = image.Clone(ctx =>
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(width, 0),   // height=0 → giữ nguyên tỉ lệ
                    Mode = ResizeMode.Max
                }));

            var variantPath = Path.Combine(uploadDir, $"{fileName}_{width}w.webp");
            var quality = width <= 400 ? 75 : 82;
            await resized.SaveAsWebpAsync(variantPath,
                new WebpEncoder { Quality = quality }, cancellationToken);
            result.Variants[width] = $"/images/{folder}/{fileName}_{width}w.webp";
        }

        return result;
    }
}
