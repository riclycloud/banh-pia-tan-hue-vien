using SPage.Domain.Enums;

namespace SPage.Application.Features.Posts.DTOs;

public sealed record PostAdminDto(
    int Id,
    string Title,
    string Slug,
    PostStatus Status,
    string CategoryName,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    int ViewCount,
    bool IsIndexable,
    string? FeaturedImageUrl = null,
    string? FeaturedImageAlt = null
);

public sealed record PostEditDto(
    int Id,
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    int CategoryId,
    List<int> TagIds,
    string? MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    string? FeaturedImageUrl,
    string? FeaturedImageAlt,
    bool IsIndexable,
    string SchemaType,
    PostStatus Status
);
