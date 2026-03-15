using MediatR;
using SPage.Application.Common.Behaviours;
using SPage.Application.Features.Pages.DTOs;

namespace SPage.Application.Features.Pages.Queries;

public sealed record GetPageBySlugQuery(string Slug)
    : IRequest<DynamicPageDto?>, ICachedQuery
{
    public string CacheKey => $"pages:slug:{Slug}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(60);
}
