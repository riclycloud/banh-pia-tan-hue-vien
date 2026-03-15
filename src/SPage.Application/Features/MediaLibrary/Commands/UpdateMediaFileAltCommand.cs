using MediatR;

namespace SPage.Application.Features.MediaLibrary.Commands;

public sealed record UpdateMediaFileAltCommand(int Id, string? AltText) : IRequest;
