using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Behaviours;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Sliders.DTOs;
using SPage.Domain.Entities.Content;

namespace SPage.Application.Features.Sliders.Queries;

public sealed record GetSliderByPositionQuery(SliderPosition Position)
    : IRequest<List<SlideApiDto>>, ICachedQuery
{
    public string CacheKey => $"slider:position:{(int)Position}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(20);
}

public sealed class GetSliderByPositionQueryHandler
    : IRequestHandler<GetSliderByPositionQuery, List<SlideApiDto>>
{
    private readonly IApplicationDbContext _db;

    public GetSliderByPositionQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<SlideApiDto>> Handle(
        GetSliderByPositionQuery request, CancellationToken cancellationToken)
    {
        return await _db.Slides
            .AsNoTracking()
            .Where(s => s.IsActive
                     && s.Slider.IsActive
                     && s.Slider.Position == request.Position)
            .OrderBy(s => s.SortOrder)
            .Select(s => new SlideApiDto(
                s.ImageUrl,
                s.AltText,
                s.Caption,
                s.LinkUrl,
                s.OpenInNewTab,
                s.SortOrder))
            .ToListAsync(cancellationToken);
    }
}
