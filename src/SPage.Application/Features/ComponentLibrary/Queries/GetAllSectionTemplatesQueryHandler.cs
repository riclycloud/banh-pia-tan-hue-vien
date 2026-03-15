using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.ComponentLibrary.DTOs;

namespace SPage.Application.Features.ComponentLibrary.Queries;

internal sealed class GetAllSectionTemplatesQueryHandler
    : IRequestHandler<GetAllSectionTemplatesQuery, List<SectionTemplateDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAllSectionTemplatesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<SectionTemplateDto>> Handle(
        GetAllSectionTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await _db.SectionTemplates
            .AsNoTracking()
            .OrderBy(t => t.SectionType)
            .ThenBy(t => t.Name)
            .Select(t => new SectionTemplateDto(
                t.Id, t.Name, t.SectionType, t.Description,
                t.DataJson, t.CssClass, t.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
