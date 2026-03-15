using MediatR;

namespace SPage.Application.Features.Posts.Commands;

public sealed record DeletePostCommand(int Id) : IRequest<bool>;
