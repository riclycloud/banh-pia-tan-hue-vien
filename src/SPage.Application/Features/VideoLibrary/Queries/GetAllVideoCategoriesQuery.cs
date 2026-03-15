using MediatR;
using SPage.Application.Features.VideoLibrary.DTOs;

namespace SPage.Application.Features.VideoLibrary.Queries;

public sealed record GetAllVideoCategoriesQuery(bool ActiveOnly = false)
    : IRequest<List<VideoCategoryDto>>;
