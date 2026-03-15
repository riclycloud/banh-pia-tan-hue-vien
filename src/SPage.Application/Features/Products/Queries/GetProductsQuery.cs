using MediatR;
using SPage.Application.Common.Behaviours;
using SPage.Application.Common.Models;
using SPage.Application.Features.Products.DTOs;

namespace SPage.Application.Features.Products.Queries;

public sealed record GetProductsQuery(
    int Page = 1,
    int PageSize = 12,
    int? CategoryId = null,
    string? CategorySlug = null,
    string? SearchTerm = null,
    string? SortBy = "newest",
    bool? IsNew = null,
    bool? IsPromotion = null
) : IRequest<PaginatedResult<ProductSummaryDto>>, ICachedQuery
{
    public string CacheKey =>
        $"products:list:p{Page}:s{PageSize}:c{CategoryId}:cs{CategorySlug}:q{SearchTerm}:sort{SortBy}:new{IsNew}:promo{IsPromotion}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(10);
}
