using MediatR;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Page;

namespace SPage.Application.Features.ComponentLibrary.Commands;

internal sealed class CreateSectionTemplateCommandHandler
    : IRequestHandler<CreateSectionTemplateCommand, int>
{
    private readonly IApplicationDbContext _db;

    public CreateSectionTemplateCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(
        CreateSectionTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new SectionTemplate
        {
            Name        = request.Name.Trim(),
            SectionType = request.SectionType,
            Description = request.Description?.Trim(),
            DataJson    = string.IsNullOrWhiteSpace(request.DataJson) ? "{}" : request.DataJson,
            CssClass    = string.IsNullOrWhiteSpace(request.CssClass) ? null : request.CssClass.Trim()
        };

        _db.SectionTemplates.Add(template);
        await _db.SaveChangesAsync(cancellationToken);
        return template.Id;
    }
}
