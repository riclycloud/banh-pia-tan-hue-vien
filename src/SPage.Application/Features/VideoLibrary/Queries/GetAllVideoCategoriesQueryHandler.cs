using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.VideoLibrary.DTOs;

namespace SPage.Application.Features.VideoLibrary.Queries;

internal sealed class GetAllVideoCategoriesQueryHandler
    : IRequestHandler<GetAllVideoCategoriesQuery, List<VideoCategoryDto>>
{
    private readonly IApplicationDbContext _db;
    public GetAllVideoCategoriesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<VideoCategoryDto>> Handle(
        GetAllVideoCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var q = _db.VideoCategories.AsNoTracking();
        if (request.ActiveOnly) q = q.Where(c => c.IsActive);

        return await q
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new VideoCategoryDto(
                c.Id, c.Name, c.Slug, c.Description, c.DisplayOrder, c.IsActive,
                c.Videos.Count(v => v.IsPublished && !v.IsDeleted)))
            .ToListAsync(cancellationToken);
    }
}
