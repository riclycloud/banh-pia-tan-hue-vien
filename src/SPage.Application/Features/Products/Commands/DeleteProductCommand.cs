using MediatR;

namespace SPage.Application.Features.Products.Commands;

public sealed record DeleteProductCommand(int Id) : IRequest<bool>;

