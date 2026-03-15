using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Posts.DTOs;
using SPage.Domain.Enums;

namespace SPage.Application.Features.Posts.Queries;

public sealed class GetPostBySlugQueryHandler
    : IRequestHandler<GetPostBySlugQuery, PostDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPostBySlugQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PostDetailDto?> Handle(
        GetPostBySlugQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Posts
            .AsNoTracking()
            .Where(p => p.Slug == request.Slug && p.Status == PostStatus.Published)
            .Include(p => p.Category)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Select(p => new PostDetailDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Content,
                p.Excerpt,
                p.FeaturedImageUrl,
                p.FeaturedImageAlt,
                p.Category.Name,
                p.Category.Slug,
                p.PublishedAt,
                p.CreatedBy ?? "Admin",
                p.PostTags.Select(pt => pt.Tag.Name),
                p.MetaTitle ?? p.Title,
                p.MetaDescription ?? p.Excerpt,
                p.CanonicalUrl,
                p.SchemaType,
                p.IsIndexable,
                p.ViewCount
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
