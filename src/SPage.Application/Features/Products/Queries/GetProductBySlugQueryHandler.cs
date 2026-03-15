using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Products.DTOs;

namespace SPage.Application.Features.Products.Queries;

public sealed class GetProductBySlugQueryHandler : IRequestHandler<GetProductBySlugQuery, ProductDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductBySlugQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProductDetailDto?> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Slug == request.Slug && p.IsActive)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.AttributeValues).ThenInclude(av => av.Attribute)
            .Select(p => new ProductDetailDto(
                p.Id,
                p.Name,
                p.Slug,
                p.Category != null ? p.Category.Slug : null,
                p.Description,
                p.ShortDescription,
                p.Price,
                p.SalePrice,
                p.SKU,
                p.IsActive,
                p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.SortOrder)
                    .Select(i => new ProductImageDto(i.ImageUrl, i.AltText, i.IsPrimary,
                        i.ImageUrl400w, i.ImageUrl800w, i.ImageUrl1200w)),
                p.AttributeValues
                    .Select(av => new ProductAttributeDto(av.Attribute.Name, av.Value)),
                p.MetaTitle ?? p.Name,
                p.MetaDescription ?? p.ShortDescription,
                p.CanonicalUrl,
                p.IsIndexable))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
