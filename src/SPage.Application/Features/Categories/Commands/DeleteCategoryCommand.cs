using MediatR;

namespace SPage.Application.Features.Categories.Commands;

public sealed record DeleteCategoryCommand(int Id) : IRequest<bool>;
