using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.VideoLibrary.Commands;

internal sealed class DeleteVideoCommandHandler : IRequestHandler<DeleteVideoCommand>
{
    private readonly IApplicationDbContext _db;
    public DeleteVideoCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteVideoCommand request, CancellationToken ct)
    {
        var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == request.Id, ct);
        if (video is null) return;
        _db.Videos.Remove(video); // AuditInterceptor → soft delete
        await _db.SaveChangesAsync(ct);
    }
}
