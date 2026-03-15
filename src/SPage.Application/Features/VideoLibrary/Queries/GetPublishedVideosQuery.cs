using MediatR;
using SPage.Application.Features.VideoLibrary.DTOs;

namespace SPage.Application.Features.VideoLibrary.Queries;

/// <summary>Lấy danh sách video đã published — dùng cho public listing</summary>
public sealed record GetPublishedVideosQuery(
    int?   CategoryId = null,
    string?Search     = null,
    int    Page       = 1,
    int    PageSize   = 12) : IRequest<PublishedVideosResult>;

public sealed record PublishedVideosResult(
    List<VideoSummaryDto> Items,
    int                   TotalCount,
    int                   Page,
    int                   PageSize,
    List<VideoCategoryDto>Categories);
