using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.MediaLibrary.DTOs;

namespace SPage.Application.Features.MediaLibrary.Queries;

internal sealed class GetAllMediaFilesQueryHandler
    : IRequestHandler<GetAllMediaFilesQuery, List<MediaFileDto>>
{
    private readonly IApplicationDbContext _db;

    public GetAllMediaFilesQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<MediaFileDto>> Handle(
        GetAllMediaFilesQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.MediaFiles
            .AsNoTracking()
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new MediaFileDto(
                f.Id,
                f.FileName,
                f.Url,
                f.FileSize,
                f.MimeType,
                f.AltText,
                f.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
