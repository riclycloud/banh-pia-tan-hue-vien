using MediatR;
using SPage.Application.Features.VideoLibrary.DTOs;

namespace SPage.Application.Features.VideoLibrary.Queries;

/// <summary>Lấy tất cả video (kể cả draft) — dùng cho admin</summary>
public sealed record GetAllVideosQuery(
    string? Search     = null,
    int?    CategoryId = null,
    int?    Platform   = null) : IRequest<List<VideoSummaryDto>>;
