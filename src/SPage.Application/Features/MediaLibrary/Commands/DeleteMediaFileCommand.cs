using MediatR;

namespace SPage.Application.Features.MediaLibrary.Commands;

public sealed record DeleteMediaFileCommand(int Id) : IRequest;
