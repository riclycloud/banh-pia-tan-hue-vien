using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.MediaLibrary.Commands;

internal sealed class UpdateMediaFileAltCommandHandler : IRequestHandler<UpdateMediaFileAltCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateMediaFileAltCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(UpdateMediaFileAltCommand request, CancellationToken cancellationToken)
    {
        var file = await _db.MediaFiles
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (file is null) return;

        file.AltText = request.AltText?.Trim();
        await _db.SaveChangesAsync(cancellationToken);
    }
}
