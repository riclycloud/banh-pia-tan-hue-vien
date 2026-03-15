using MediatR;

namespace SPage.Application.Features.VideoLibrary.Commands;

public sealed record DeleteVideoCommand(int Id) : IRequest;
