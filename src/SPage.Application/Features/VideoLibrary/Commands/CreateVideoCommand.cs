using MediatR;
using SPage.Domain.Enums;

namespace SPage.Application.Features.VideoLibrary.Commands;

public sealed record CreateVideoCommand(
    string         Title,
    VideoPlatform? Platform,
    string         VideoUrl,
    string?        VideoId       = null,
    string?        ThumbnailUrl  = null,
    string?        Description   = null,
    string?        Duration      = null,
    int?           CategoryId    = null,
    string?        TagsCsv       = null,
    string?        MetaTitle     = null,
    string?        MetaDescription = null,
    bool           IsIndexable   = true,
    bool           IsFeatured    = false,
    bool           IsPublished   = false,
    string?        Slug          = null) : IRequest<int>;
