using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.ComponentLibrary.DTOs;

namespace SPage.Application.Features.ComponentLibrary.Queries;

internal sealed class GetSectionTemplateByIdQueryHandler
    : IRequestHandler<GetSectionTemplateByIdQuery, SectionTemplateDto?>
{
    private readonly IApplicationDbContext _db;

    public GetSectionTemplateByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<SectionTemplateDto?> Handle(
        GetSectionTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.SectionTemplates
            .AsNoTracking()
            .Where(t => t.Id == request.Id)
            .Select(t => new SectionTemplateDto(
                t.Id, t.Name, t.SectionType, t.Description,
                t.DataJson, t.CssClass, t.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
