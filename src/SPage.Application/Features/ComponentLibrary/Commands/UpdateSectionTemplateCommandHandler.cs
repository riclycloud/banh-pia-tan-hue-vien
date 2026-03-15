using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.ComponentLibrary.Commands;

internal sealed class UpdateSectionTemplateCommandHandler
    : IRequestHandler<UpdateSectionTemplateCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateSectionTemplateCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(
        UpdateSectionTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _db.SectionTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"SectionTemplate {request.Id} not found.");

        template.Name        = request.Name.Trim();
        template.SectionType = request.SectionType;
        template.Description = request.Description?.Trim();
        template.DataJson    = string.IsNullOrWhiteSpace(request.DataJson) ? "{}" : request.DataJson;
        template.CssClass    = string.IsNullOrWhiteSpace(request.CssClass) ? null : request.CssClass.Trim();

        await _db.SaveChangesAsync(cancellationToken);
    }
}
