using MediatR;

namespace SPage.Application.Features.ProductCategories.Commands;

public sealed record UpdateProductCategoryCommand(
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
    string? ThumbnailAlt
) : IRequest<bool>;
