using MediatR;

namespace SPage.Application.Features.VideoLibrary.Commands;

public sealed record DeleteVideoCategoryCommand(int Id) : IRequest;
