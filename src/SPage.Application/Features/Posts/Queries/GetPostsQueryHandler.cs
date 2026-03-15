using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Common.Models;
using SPage.Application.Features.Posts.DTOs;
using SPage.Domain.Enums;

namespace SPage.Application.Features.Posts.Queries;

public sealed class GetPostsQueryHandler
    : IRequestHandler<GetPostsQuery, PaginatedResult<PostSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPostsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedResult<PostSummaryDto>> Handle(
        GetPostsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Posts
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Published)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .AsQueryable();

        // Nếu có CategorySlug thì resolve sang ID (ưu tiên CategoryId nếu đã truyền)
        if (!request.CategoryId.HasValue && !string.IsNullOrEmpty(request.CategorySlug))
        {
            var catId = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Slug == request.CategorySlug)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (catId.HasValue)
                query = query.Where(p => p.CategoryId == catId.Value);
        }
        else if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrEmpty(request.TagSlug))
            query = query.Where(p => p.PostTags.Any(pt => pt.Tag.Slug == request.TagSlug));

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                (p.Excerpt != null && p.Excerpt.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Projection trực tiếp ra DTO — không load toàn bộ entity
        var items = await query
            .OrderByDescending(p => p.PublishedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PostSummaryDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Excerpt,
                p.FeaturedImageUrl,
                p.FeaturedImageAlt,
                p.Category.Name,
                p.Category.Slug,
                p.PublishedAt,
                p.PostTags.Select(pt => pt.Tag.Name),
                p.MetaTitle ?? p.Title,
                p.MetaDescription ?? p.Excerpt,
                p.SchemaType,
                p.IsIndexable
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PostSummaryDto>(items, totalCount, request.Page, request.PageSize);
    }
}
