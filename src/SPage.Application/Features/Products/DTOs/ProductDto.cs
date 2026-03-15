namespace SPage.Application.Features.Products.DTOs;

public sealed record ProductSummaryDto(
    int Id,
    string Name,
    string Slug,
    string? CategorySlug,
    string? ShortDescription,
    decimal Price,
    decimal? SalePrice,
    string? PrimaryImageUrl,
    string? PrimaryImageAlt,
    string MetaTitle,
    string? MetaDescription,
    bool IsActive,
    bool IsNew,
    bool IsFeatured,
    bool IsPromotion
);

public sealed record ProductDetailDto(
    int Id,
    string Name,
    string Slug,
    string? CategorySlug,
    string? Description,
    string? ShortDescription,
    decimal Price,
    decimal? SalePrice,
    string? SKU,
    bool IsActive,
    IEnumerable<ProductImageDto> Images,
    IEnumerable<ProductAttributeDto> Attributes,
    string MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    bool IsIndexable
);

public sealed record ProductImageDto(
    string ImageUrl,
    string? AltText,
    bool IsPrimary,
    string? ImageUrl400w,
    string? ImageUrl800w,
    string? ImageUrl1200w
);

public sealed record ProductAttributeDto(string Name, string Value);
