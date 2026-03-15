using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Categories.DTOs;

namespace SPage.Application.Features.Categories.Queries;

public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDetailDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c => new CategoryDetailDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.ParentId,
                c.MetaTitle,
                c.MetaDescription,
                c.CanonicalUrl,
                c.IsIndexable,
                c.ThumbnailUrl,
                c.ThumbnailAlt,
                c.BannerUrl,
                c.BannerAlt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

