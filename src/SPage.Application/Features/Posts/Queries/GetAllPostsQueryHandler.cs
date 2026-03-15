using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Common.Models;
using SPage.Application.Features.Posts.DTOs;

namespace SPage.Application.Features.Posts.Queries;

public sealed class GetAllPostsQueryHandler
    : IRequestHandler<GetAllPostsQuery, PaginatedResult<PostAdminDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllPostsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedResult<PostAdminDto>> Handle(
        GetAllPostsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Posts
            .AsNoTracking()
            .IgnoreQueryFilters()   // bỏ soft-delete filter để thấy tất cả
            .Include(p => p.Category)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => p.Title.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PostAdminDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Status,
                p.Category.Name,
                p.PublishedAt,
                p.CreatedAt,
                p.ViewCount,
                p.IsIndexable,
                p.FeaturedImageUrl,
                p.FeaturedImageAlt))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PostAdminDto>(items, total, request.Page, request.PageSize);
    }
}
