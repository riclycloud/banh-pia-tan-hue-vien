using MediatR;
using SPage.Domain.Enums;

namespace SPage.Application.Features.Posts.Commands;

public sealed record UpdatePostCommand(
    int Id,
    string Title,
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
    PostStatus Status,
    string? Slug = null
) : IRequest<bool>;
