using SPage.Domain.Entities.Page;

namespace SPage.Application.Features.Pages.DTOs;

public sealed record DynamicPageDto(
    int Id,
    string Title,
    string Slug,
    string Template,
    IEnumerable<PageSectionData> Sections,
    string MetaTitle,
    string? MetaDescription,
    string? CanonicalUrl,
    bool IsIndexable
);
