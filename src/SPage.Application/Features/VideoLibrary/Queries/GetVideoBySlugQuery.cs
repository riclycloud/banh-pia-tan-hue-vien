using MediatR;
using SPage.Application.Features.VideoLibrary.DTOs;

namespace SPage.Application.Features.VideoLibrary.Queries;

/// <summary>Lấy chi tiết video theo slug — public detail page</summary>
public sealed record GetVideoBySlugQuery(string Slug) : IRequest<VideoDetailDto?>;
