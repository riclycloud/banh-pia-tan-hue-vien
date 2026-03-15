using MediatR;
using SPage.Application.Features.VideoLibrary.DTOs;

namespace SPage.Application.Features.VideoLibrary.Queries;

/// <summary>Lấy video đã published nhóm theo danh mục — mỗi section một danh mục. Nếu CategorySlug set thì chỉ trả về một danh mục đó.</summary>
public sealed record GetPublishedVideosGroupedByCategoryQuery(
    string? Search = null,
    int     MaxVideosPerCategory = 12,
    string? CategorySlug = null) : IRequest<VideosGroupedByCategoryResult>;

public sealed record VideosGroupedByCategoryResult(
    List<VideoCategorySectionDto> Sections);

public sealed record VideoCategorySectionDto(
    VideoCategoryDto Category,
    List<VideoSummaryDto> Videos);
