using MediatR;

namespace SPage.Application.Features.Products.Commands;

public sealed record CreateProductCommand(
    string Name,
    string? Slug,
    string? Description,
    string? ShortDescription,
    decimal Price,
    decimal? SalePrice,
    string? SKU,
    bool IsActive,
    bool IsNew,
    bool IsFeatured,
    bool IsPromotion,
    int SortOrder,
    int CategoryId,
    string? MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    bool IsIndexable,
    string? PrimaryImageUrl,
    string? PrimaryImageAlt
) : IRequest<int>;

