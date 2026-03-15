using MediatR;
using SPage.Application.Common.Behaviours;
using SPage.Application.Features.Posts.DTOs;

namespace SPage.Application.Features.Posts.Queries;

public sealed record GetPostBySlugQuery(string Slug)
    : IRequest<PostDetailDto?>, ICachedQuery
{
    public string CacheKey => $"posts:slug:{Slug}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(30);
}
