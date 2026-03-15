namespace SPage.Application.Features.ComponentLibrary.DTOs;

public sealed record SectionTemplateDto(
    int Id,
    string Name,
    string SectionType,
    string? Description,
    string DataJson,
    string? CssClass,
    DateTimeOffset CreatedAt
);
