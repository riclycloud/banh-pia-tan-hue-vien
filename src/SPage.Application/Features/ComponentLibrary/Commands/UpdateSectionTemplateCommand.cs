using MediatR;

namespace SPage.Application.Features.ComponentLibrary.Commands;

public sealed record UpdateSectionTemplateCommand(
    int Id,
    string Name,
    string SectionType,
    string? Description,
    string DataJson,
    string? CssClass
) : IRequest;
