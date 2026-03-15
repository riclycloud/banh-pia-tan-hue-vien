using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.ProductCategories.DTOs;

namespace SPage.Application.Features.ProductCategories.Queries;

public sealed class GetProductCategoryByIdQueryHandler
    : IRequestHandler<GetProductCategoryByIdQuery, ProductCategoryDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductCategoryByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProductCategoryDetailDto?> Handle(
        GetProductCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var c = await _context.ProductCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (c is null) return null;

        return new ProductCategoryDetailDto(
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
            c.ThumbnailAlt);
    }
}
