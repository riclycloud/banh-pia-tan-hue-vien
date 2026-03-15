using MediatR;

namespace SPage.Application.Features.ComponentLibrary.Commands;

public sealed record CreateSectionTemplateCommand(
    string Name,
    string SectionType,
    string? Description,
    string DataJson,
    string? CssClass
) : IRequest<int>;
