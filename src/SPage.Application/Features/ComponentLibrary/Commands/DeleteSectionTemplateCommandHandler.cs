using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.ComponentLibrary.Commands;

internal sealed class DeleteSectionTemplateCommandHandler
    : IRequestHandler<DeleteSectionTemplateCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteSectionTemplateCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(
        DeleteSectionTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _db.SectionTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (template is not null)
        {
            _db.SectionTemplates.Remove(template); // AuditInterceptor → soft delete
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
