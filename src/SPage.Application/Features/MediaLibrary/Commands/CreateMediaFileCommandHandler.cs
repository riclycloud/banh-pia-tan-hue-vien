using MediatR;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Media;

namespace SPage.Application.Features.MediaLibrary.Commands;

internal sealed class CreateMediaFileCommandHandler
    : IRequestHandler<CreateMediaFileCommand, int>
{
    private readonly IApplicationDbContext _db;

    public CreateMediaFileCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(
        CreateMediaFileCommand request,
        CancellationToken cancellationToken)
    {
        var file = new MediaFile
        {
            FileName   = request.FileName,
            StoredName = request.StoredName,
            Url        = request.Url,
            FileSize   = request.FileSize,
            MimeType   = request.MimeType
        };

        _db.MediaFiles.Add(file);
        await _db.SaveChangesAsync(cancellationToken);
        return file.Id;
    }
}
