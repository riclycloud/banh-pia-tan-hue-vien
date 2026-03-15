namespace SPage.Application.Features.Categories.DTOs;

public sealed record CategoryDetailDto(
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
    string? ThumbnailAlt,
    string? BannerUrl,
    string? BannerAlt
);

