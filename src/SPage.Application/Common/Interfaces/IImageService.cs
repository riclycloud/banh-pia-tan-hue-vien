using Microsoft.AspNetCore.Http;

namespace SPage.Application.Common.Interfaces;

public interface IImageService
{
    Task<ImageUploadResult> ProcessAndSaveAsync(
        IFormFile file,
        string folder,
        CancellationToken cancellationToken = default);
}

public sealed class ImageUploadResult
{
    public string OriginalUrl { get; set; } = string.Empty;
    public Dictionary<int, string> Variants { get; set; } = [];  // width -> url
}
