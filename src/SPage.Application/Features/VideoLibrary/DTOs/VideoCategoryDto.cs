namespace SPage.Application.Features.VideoLibrary.DTOs;

public sealed record VideoCategoryDto(
    int     Id,
    string  Name,
    string  Slug,
    string? Description,
    int     DisplayOrder,
    bool    IsActive,
    int     VideoCount);
