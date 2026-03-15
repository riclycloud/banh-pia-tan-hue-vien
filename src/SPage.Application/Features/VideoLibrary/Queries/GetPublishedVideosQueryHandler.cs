using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.VideoLibrary.DTOs;
using SPage.Domain.Enums;

namespace SPage.Application.Features.VideoLibrary.Queries;

internal sealed class GetPublishedVideosQueryHandler
    : IRequestHandler<GetPublishedVideosQuery, PublishedVideosResult>
{
    private readonly IApplicationDbContext _db;
    public GetPublishedVideosQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PublishedVideosResult> Handle(
        GetPublishedVideosQuery request,
        CancellationToken cancellationToken)
    {
        var q = _db.Videos
            .AsNoTracking()
            .Where(v => v.IsPublished)
            .Include(v => v.Category)
            .AsQueryable();

        if (request.CategoryId.HasValue)
            q = q.Where(v => v.CategoryId == request.CategoryId);

        if (!string.IsNullOrWhiteSpace(request.Search))
            q = q.Where(v => v.Title.Contains(request.Search));

        var total = await q.CountAsync(cancellationToken);

        var pageSize = Math.Max(1, Math.Min(request.PageSize, 48));
        var page     = Math.Max(1, request.Page);
        var skip     = (page - 1) * pageSize;

        var items = await q
            .OrderByDescending(v => v.IsFeatured)
            .ThenByDescending(v => v.PublishedAt)
            .Skip(skip)
            .Take(pageSize)
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
                v.Category != null ? v.Category.Name : null,
                v.Category != null ? v.Category.Slug : null,
                v.PublishedAt,
                v.IsFeatured,
                v.ViewCount,
                v.IsPublished))
            .ToListAsync(cancellationToken);

        // Categories with video count for filter tabs
        var categories = await _db.VideoCategories
            .AsNoTracking()
            .Where(c => c.IsActive && c.Videos.Any(v => v.IsPublished))
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new VideoCategoryDto(
                c.Id, c.Name, c.Slug, c.Description, c.DisplayOrder, c.IsActive,
                c.Videos.Count(v => v.IsPublished)))
            .ToListAsync(cancellationToken);

        return new PublishedVideosResult(items, total, page, pageSize, categories);
    }
}
