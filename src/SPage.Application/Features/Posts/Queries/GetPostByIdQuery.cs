using MediatR;
using SPage.Application.Features.Posts.DTOs;

namespace SPage.Application.Features.Posts.Queries;

public sealed record GetPostByIdQuery(int Id) : IRequest<PostEditDto?>;
