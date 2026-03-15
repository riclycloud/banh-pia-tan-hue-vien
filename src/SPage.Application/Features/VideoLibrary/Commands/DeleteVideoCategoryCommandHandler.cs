using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.VideoLibrary.Commands;

internal sealed class DeleteVideoCategoryCommandHandler : IRequestHandler<DeleteVideoCategoryCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteVideoCategoryCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteVideoCategoryCommand request, CancellationToken ct)
    {
        var cat = await _db.VideoCategories.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
        if (cat is null) return;
        _db.VideoCategories.Remove(cat); // AuditInterceptor → soft delete
        await _db.SaveChangesAsync(ct);
    }
}
