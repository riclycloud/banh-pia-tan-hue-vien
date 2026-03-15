using MediatR;
using SPage.Domain.Enums;

namespace SPage.Application.Features.Posts.Commands;

public sealed record CreatePostCommand(
    string Title,
    string Content,
    string? Excerpt,
    int CategoryId,
    List<int> TagIds,
    string? MetaTitle,
    string? MetaDescription,
    string? FeaturedImageUrl,
    string? FeaturedImageAlt,
    bool IsIndexable = true,
    string SchemaType = "Article",
    string? Slug = null,
    PostStatus Status = PostStatus.Draft
) : IRequest<int>;
