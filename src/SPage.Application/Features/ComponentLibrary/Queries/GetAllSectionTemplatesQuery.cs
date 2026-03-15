using MediatR;
using SPage.Application.Features.ComponentLibrary.DTOs;

namespace SPage.Application.Features.ComponentLibrary.Queries;

public sealed record GetAllSectionTemplatesQuery : IRequest<List<SectionTemplateDto>>;
