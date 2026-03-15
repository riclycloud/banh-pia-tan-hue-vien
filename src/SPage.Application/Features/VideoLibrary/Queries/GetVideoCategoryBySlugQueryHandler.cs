using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.VideoLibrary.DTOs;

namespace SPage.Application.Features.VideoLibrary.Queries;

internal sealed class GetVideoCategoryBySlugQueryHandler
    : IRequestHandler<GetVideoCategoryBySlugQuery, VideoCategoryDto?>
{
    private readonly IApplicationDbContext _db;
    public GetVideoCategoryBySlugQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<VideoCategoryDto?> Handle(
        GetVideoCategoryBySlugQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Slug)) return null;

        var cat = await _db.VideoCategories
            .AsNoTracking()
            .Where(c => c.Slug == request.Slug && c.IsActive)
            .Select(c => new VideoCategoryDto(
                c.Id, c.Name, c.Slug, c.Description, c.DisplayOrder, c.IsActive,
                c.Videos.Count(v => v.IsPublished)))
            .FirstOrDefaultAsync(cancellationToken);

        return cat;
    }
}
