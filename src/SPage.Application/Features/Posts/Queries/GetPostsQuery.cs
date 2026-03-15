using MediatR;
using SPage.Application.Common.Behaviours;
using SPage.Application.Common.Models;
using SPage.Application.Features.Posts.DTOs;

namespace SPage.Application.Features.Posts.Queries;

public sealed record GetPostsQuery(
    int Page = 1,
    int PageSize = 10,
    int? CategoryId = null,
    string? CategorySlug = null,
    string? TagSlug = null,
    string? SearchTerm = null
) : IRequest<PaginatedResult<PostSummaryDto>>, ICachedQuery
{
    public string CacheKey =>
        $"posts:list:p{Page}:s{PageSize}:c{CategoryId}:cs{CategorySlug}:t{TagSlug}:q{SearchTerm}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(15);
}
