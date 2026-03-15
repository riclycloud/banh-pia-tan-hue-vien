using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.VideoLibrary.DTOs;
using SPage.Domain.Enums;

namespace SPage.Application.Features.VideoLibrary.Queries;

internal sealed class GetAllVideosQueryHandler
    : IRequestHandler<GetAllVideosQuery, List<VideoSummaryDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAllVideosQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<VideoSummaryDto>> Handle(
        GetAllVideosQuery request,
        CancellationToken cancellationToken)
    {
        var q = _db.Videos
            .AsNoTracking()
            .Include(v => v.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            q = q.Where(v => v.Title.Contains(request.Search) || v.VideoId.Contains(request.Search));

        if (request.CategoryId.HasValue)
            q = q.Where(v => v.CategoryId == request.CategoryId);

        if (request.Platform.HasValue)
            q = q.Where(v => (int)v.Platform == request.Platform.Value);

        return await q
            .OrderByDescending(v => v.CreatedAt)
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
    }
}
