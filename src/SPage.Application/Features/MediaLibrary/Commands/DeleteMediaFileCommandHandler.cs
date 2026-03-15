using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.MediaLibrary.Commands;

internal sealed class DeleteMediaFileCommandHandler : IRequestHandler<DeleteMediaFileCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteMediaFileCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteMediaFileCommand request, CancellationToken cancellationToken)
    {
        var file = await _db.MediaFiles
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (file is null) return;

        _db.MediaFiles.Remove(file); // AuditInterceptor → soft delete (IsDeleted = true)
        await _db.SaveChangesAsync(cancellationToken);
    }
}
