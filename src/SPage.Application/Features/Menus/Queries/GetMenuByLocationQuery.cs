using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Behaviours;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Menus.DTOs;
using SPage.Domain.Entities.Navigation;

namespace SPage.Application.Features.Menus.Queries;

public sealed record GetMenuByLocationQuery(string LocationKey)
    : IRequest<List<MenuItemApiDto>>, ICachedQuery
{
    public string CacheKey => $"menu:location:{LocationKey}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(30);
}

public sealed class GetMenuByLocationQueryHandler
    : IRequestHandler<GetMenuByLocationQuery, List<MenuItemApiDto>>
{
    private readonly IApplicationDbContext _db;

    public GetMenuByLocationQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<MenuItemApiDto>> Handle(
        GetMenuByLocationQuery request, CancellationToken cancellationToken)
    {
        var items = await _db.MenuItems
            .AsNoTracking()
            .Include(x => x.Children.Where(c => c.IsActive))
            .Where(x => x.IsActive
                     && x.ParentId == null
                     && x.Location!.Key == request.LocationKey
                     && x.Location.IsActive)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return items.Select(MapToDto).ToList();
    }

    private static MenuItemApiDto MapToDto(MenuItem item) => new(
        item.Title,
        item.Url,
        item.OpenInNewTab,
        item.Children
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(MapToDto)
            .ToList()
    );
}
