using MediatR;

namespace SPage.Application.Features.Categories.Commands;

public sealed record UpdateCategoryCommand(
    int Id,
    string Name,
    string? Slug,
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
) : IRequest<bool>;

