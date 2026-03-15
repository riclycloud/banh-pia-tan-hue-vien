namespace SPage.Application.Features.Categories.DTOs;

public sealed record CategoryDto(
    int Id,
    string Name,
    string Slug,
    int? ParentId,
    string? ParentName,
    int PostCount
);
