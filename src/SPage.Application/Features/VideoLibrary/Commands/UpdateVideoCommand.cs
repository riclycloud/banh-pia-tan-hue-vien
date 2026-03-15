using MediatR;
using SPage.Domain.Enums;

namespace SPage.Application.Features.VideoLibrary.Commands;

public sealed record UpdateVideoCommand(
    int            Id,
    string         Title,
    VideoPlatform  Platform,
    string         VideoUrl,
    string         VideoId,
    string?        ThumbnailUrl,
    string?        Description,
    string?        Duration,
    int?           CategoryId,
    string?        TagsCsv,
    string?        MetaTitle,
    string?        MetaDescription,
    bool           IsIndexable,
    bool           IsFeatured,
    bool           IsPublished,
    string?        Slug = null) : IRequest;
