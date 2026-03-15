using MediatR;

namespace SPage.Application.Features.MediaLibrary.Commands;

public sealed record CreateMediaFileCommand(
    string FileName,
    string StoredName,
    string Url,
    long   FileSize,
    string MimeType) : IRequest<int>;
