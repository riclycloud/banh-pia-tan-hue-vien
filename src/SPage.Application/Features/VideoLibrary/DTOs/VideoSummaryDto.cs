using SPage.Domain.Enums;

namespace SPage.Application.Features.VideoLibrary.DTOs;

public sealed record VideoSummaryDto(
    int            Id,
    string         Title,
    string         Slug,
    VideoPlatform  Platform,
    string         VideoId,
    string         ThumbnailUrl,
    string?        CategoryName,
    string?        CategorySlug,
    DateTimeOffset?PublishedAt,
    bool           IsFeatured,
    int            ViewCount,
    bool           IsPublished = false);
