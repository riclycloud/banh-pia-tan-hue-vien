namespace SPage.Application.Features.ProductCategories.DTOs;

public sealed record ProductCategoryDto(
    int Id,
    string Name,
    string Slug,
    int? ParentId,
    string? ParentName,
    int ProductCount
);

public sealed record ProductCategoryDetailDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    int? ParentId,
    string? MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    bool IsIndexable,
    string? ThumbnailUrl,
    string? ThumbnailAlt
);
