using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.ProductCategories.DTOs;

namespace SPage.Application.Features.ProductCategories.Queries;

public sealed class GetProductCategoriesQueryHandler
    : IRequestHandler<GetProductCategoriesQuery, List<ProductCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductCategoriesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ProductCategoryDto>> Handle(
        GetProductCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _context.ProductCategories
            .AsNoTracking()
            .Include(c => c.Parent)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var productCounts = await _context.Products
            .AsNoTracking()
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CategoryId, g => g.Count, cancellationToken);

        return categories
            .Select(c => new ProductCategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.ParentId,
                c.Parent?.Name,
                productCounts.GetValueOrDefault(c.Id, 0)))
            .ToList();
    }
}
