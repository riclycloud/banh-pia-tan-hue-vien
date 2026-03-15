using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.VideoLibrary.DTOs;
using SPage.Domain.Enums;

namespace SPage.Application.Features.VideoLibrary.Queries;

internal sealed class GetVideoBySlugQueryHandler
    : IRequestHandler<GetVideoBySlugQuery, VideoDetailDto?>
{
    private readonly IApplicationDbContext _db;
    public GetVideoBySlugQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<VideoDetailDto?> Handle(
        GetVideoBySlugQuery request,
        CancellationToken cancellationToken)
    {
        // Load with tracking so we can increment ViewCount — so sánh slug không phân biệt hoa thường
        var slugLower = request.Slug.Trim().ToLowerInvariant();
        var v = await _db.Videos
            .Include(x => x.Category)
            .Where(x => x.Slug.ToLower() == slugLower && x.IsPublished)
            .FirstOrDefaultAsync(cancellationToken);

        if (v is null) return null;

        // Increment ViewCount
        v.ViewCount++;
        await _db.SaveChangesAsync(cancellationToken);

        var thumbnail = !string.IsNullOrEmpty(v.ThumbnailUrl)
            ? v.ThumbnailUrl
            : v.Platform == VideoPlatform.YouTube
                ? $"https://img.youtube.com/vi/{v.VideoId}/maxresdefault.jpg"
                : "/img/video-placeholder.svg";

        var embedUrl = v.Platform switch
        {
            VideoPlatform.YouTube  => $"https://www.youtube.com/embed/{v.VideoId}?rel=0&modestbranding=1",
            VideoPlatform.Facebook => $"https://www.facebook.com/plugins/video.php?href={Uri.EscapeDataString(v.VideoUrl)}&show_text=0",
            VideoPlatform.TikTok   => $"https://www.tiktok.com/embed/v2/{v.VideoId}",
            _                      => v.VideoUrl
        };

        return new VideoDetailDto(
            v.Id, v.Title, v.Slug, v.Description,
            v.Platform, v.VideoId, v.VideoUrl,
            thumbnail, embedUrl, v.Duration, v.TagsCsv,
            v.IsPublished, v.PublishedAt, v.IsFeatured, v.ViewCount,
            v.CategoryId,
            v.Category?.Name,
            v.Category?.Slug,
            v.MetaTitle, v.MetaDescription, v.CanonicalUrl, v.IsIndexable,
            v.CreatedAt);
    }
}
