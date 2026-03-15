using MediatR;
using SPage.Application.Features.VideoLibrary.DTOs;

namespace SPage.Application.Features.VideoLibrary.Queries;

/// <summary>Lấy danh mục video theo slug — dùng để phân giải /video/{slug} (danh mục hoặc video).</summary>
public sealed record GetVideoCategoryBySlugQuery(string Slug) : IRequest<VideoCategoryDto?>;
