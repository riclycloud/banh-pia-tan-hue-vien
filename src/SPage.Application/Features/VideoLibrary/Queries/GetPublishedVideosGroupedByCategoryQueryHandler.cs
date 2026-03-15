using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.VideoLibrary.DTOs;
using SPage.Domain.Enums;

namespace SPage.Application.Features.VideoLibrary.Queries;

internal sealed class GetPublishedVideosGroupedByCategoryQueryHandler
    : IRequestHandler<GetPublishedVideosGroupedByCategoryQuery, VideosGroupedByCategoryResult>
{
    private readonly IApplicationDbContext _db;

    public GetPublishedVideosGroupedByCategoryQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<VideosGroupedByCategoryResult> Handle(
        GetPublishedVideosGroupedByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim();
        var maxPer = Math.Max(1, Math.Min(request.MaxVideosPerCategory, 24));

        var catQuery = _db.VideoCategories
            .AsNoTracking()
            .Where(c => c.IsActive && c.Videos.Any(v => v.IsPublished));

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
            catQuery = catQuery.Where(c => c.Slug == request.CategorySlug);

        var categories = await catQuery
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Slug, c.Description, c.DisplayOrder, c.IsActive })
            .ToListAsync(cancellationToken);

        var sections = new List<VideoCategorySectionDto>();

        foreach (var cat in categories)
        {
            var q = _db.Videos
                .AsNoTracking()
                .Where(v => v.IsPublished && v.CategoryId == cat.Id);

            if (!string.IsNullOrEmpty(search))
                q = q.Where(v => v.Title.Contains(search));

            var videos = await q
                .OrderByDescending(v => v.IsFeatured)
                .ThenByDescending(v => v.PublishedAt)
                .Take(maxPer)
                .Select(v => new VideoSummaryDto(
                    v.Id,
                    v.Title,
                    v.Slug,
                    v.Platform,
                    v.VideoId,
                    v.ThumbnailUrl != null ? v.ThumbnailUrl
                        : v.Platform == VideoPlatform.YouTube
                            ? "https://img.youtube.com/vi/" + v.VideoId + "/mqdefault.jpg"
                            : "/img/video-placeholder.svg",
                    cat.Name,
                    cat.Slug,
                    v.PublishedAt,
                    v.IsFeatured,
                    v.ViewCount,
                    true))
                .ToListAsync(cancellationToken);

            // Khi lọc theo CategorySlug vẫn hiển thị section dù 0 video
            if (videos.Count == 0 && string.IsNullOrWhiteSpace(request.CategorySlug)) continue;

            var categoryDto = new VideoCategoryDto(
                cat.Id, cat.Name, cat.Slug, cat.Description, cat.DisplayOrder, cat.IsActive, videos.Count);
            sections.Add(new VideoCategorySectionDto(categoryDto, videos));
        }

        // Section "Không phân loại" nếu có video không thuộc danh mục
        var uncategorizedQuery = _db.Videos
            .AsNoTracking()
            .Where(v => v.IsPublished && v.CategoryId == null);

        if (!string.IsNullOrEmpty(search))
            uncategorizedQuery = uncategorizedQuery.Where(v => v.Title.Contains(search));

        var uncategorized = await uncategorizedQuery
            .OrderByDescending(v => v.IsFeatured)
            .ThenByDescending(v => v.PublishedAt)
            .Take(maxPer)
            .Select(v => new VideoSummaryDto(
                v.Id,
                v.Title,
                v.Slug,
                v.Platform,
                v.VideoId,
                v.ThumbnailUrl != null ? v.ThumbnailUrl
                    : v.Platform == VideoPlatform.YouTube
                        ? "https://img.youtube.com/vi/" + v.VideoId + "/mqdefault.jpg"
                        : "/img/video-placeholder.svg",
                null,
                null,
                v.PublishedAt,
                v.IsFeatured,
                v.ViewCount,
                true))
            .ToListAsync(cancellationToken);

        if (uncategorized.Count > 0)
        {
            var otherCat = new VideoCategoryDto(0, "Khác", "khac", null, 999, true, uncategorized.Count);
            sections.Add(new VideoCategorySectionDto(otherCat, uncategorized));
        }

        return new VideosGroupedByCategoryResult(sections);
    }
}
