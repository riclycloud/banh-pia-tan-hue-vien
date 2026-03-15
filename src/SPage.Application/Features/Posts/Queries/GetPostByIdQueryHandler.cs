using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Posts.DTOs;

namespace SPage.Application.Features.Posts.Queries;

public sealed class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, PostEditDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPostByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PostEditDto?> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Posts
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(p => p.PostTags)
            .Where(p => p.Id == request.Id)
            .Select(p => new PostEditDto(
                p.Id,
                p.Title,
                p.Slug,
                p.Content,
                p.Excerpt,
                p.CategoryId,
                p.PostTags.Select(pt => pt.TagId).ToList(),
                p.MetaTitle,
                p.MetaDescription,
                p.CanonicalUrl,
                p.FeaturedImageUrl,
                p.FeaturedImageAlt,
                p.IsIndexable,
                p.SchemaType,
                p.Status))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
