namespace SPage.Application.Features.Posts.DTOs;

public sealed record PostSummaryDto(
    int Id,
    string Title,
    string Slug,
    string? Excerpt,
    string? FeaturedImageUrl,
    string? FeaturedImageAlt,
    string CategoryName,
    string CategorySlug,
    DateTimeOffset? PublishedAt,
    IEnumerable<string> Tags,
    string MetaTitle,
    string? MetaDescription,
    string SchemaType,
    bool IsIndexable
);

public sealed record PostDetailDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    string? FeaturedImageUrl,
    string? FeaturedImageAlt,
    string CategoryName,
    string CategorySlug,
    DateTimeOffset? PublishedAt,
    string CreatedBy,
    IEnumerable<string> Tags,
    string MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    string SchemaType,
    bool IsIndexable,
    int ViewCount
);
