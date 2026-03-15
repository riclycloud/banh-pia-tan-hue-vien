using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Common.Models;
using SPage.Application.Features.Products.DTOs;

namespace SPage.Application.Features.Products.Queries;

public sealed class GetProductsQueryHandler
    : IRequestHandler<GetProductsQuery, PaginatedResult<ProductSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedResult<ProductSummaryDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable();

        // Nếu có CategorySlug thì resolve sang ID (ưu tiên CategoryId nếu đã truyền)
        if (!request.CategoryId.HasValue && !string.IsNullOrEmpty(request.CategorySlug))
        {
            var catId = await _context.ProductCategories
                .AsNoTracking()
                .Where(c => c.Slug == request.CategorySlug)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (catId.HasValue)
                query = query.Where(p => p.CategoryId == catId.Value);
        }
        else if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.IsNew.HasValue)
            query = query.Where(p => p.IsNew == request.IsNew.Value);

        if (request.IsPromotion.HasValue)
            query = query.Where(p => p.IsPromotion == request.IsPromotion.Value);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        query = request.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.SalePrice ?? p.Price),
            "price_desc" => query.OrderByDescending(p => p.SalePrice ?? p.Price),
            "name_asc" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductSummaryDto(
                p.Id,
                p.Name,
                p.Slug,
                p.Category != null ? p.Category.Slug : null,
                p.ShortDescription,
                p.Price,
                p.SalePrice,
                p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.SortOrder).Select(i => i.AltText).FirstOrDefault(),
                p.MetaTitle ?? p.Name,
                p.MetaDescription ?? p.ShortDescription,
                p.IsActive,
                p.IsNew,
                p.IsFeatured,
                p.IsPromotion
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ProductSummaryDto>(items, totalCount, request.Page, request.PageSize);
    }
}
