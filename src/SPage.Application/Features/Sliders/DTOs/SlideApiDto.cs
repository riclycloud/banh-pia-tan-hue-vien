namespace SPage.Application.Features.Sliders.DTOs;

public sealed record SlideApiDto(
    string ImageUrl,
    string? AltText,
    string? Caption,
    string? LinkUrl,
    bool OpenInNewTab,
    int SortOrder
);
