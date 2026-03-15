using MediatR;
using SPage.Application.Common.Models;
using SPage.Application.Features.Posts.DTOs;
using SPage.Domain.Enums;

namespace SPage.Application.Features.Posts.Queries;

/// <summary>Admin query — trả về tất cả bài viết kể cả Draft/Scheduled.</summary>
public sealed record GetAllPostsQuery(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    PostStatus? Status = null
) : IRequest<PaginatedResult<PostAdminDto>>;
