using SPage.Domain.Enums;

namespace SPage.Application.Features.VideoLibrary.DTOs;

public sealed record VideoDetailDto(
    int            Id,
    string         Title,
    string         Slug,
    string?        Description,
    VideoPlatform  Platform,
    string         VideoId,
    string         VideoUrl,
    string         ThumbnailUrl,
    string         EmbedUrl,
    string?        Duration,
    string?        TagsCsv,
    bool           IsPublished,
    DateTimeOffset?PublishedAt,
    bool           IsFeatured,
    int            ViewCount,
    int?           CategoryId,
    string?        CategoryName,
    string?        CategorySlug,
    string?        MetaTitle,
    string?        MetaDescription,
    string?        CanonicalUrl,
    bool           IsIndexable,
    DateTimeOffset CreatedAt);
