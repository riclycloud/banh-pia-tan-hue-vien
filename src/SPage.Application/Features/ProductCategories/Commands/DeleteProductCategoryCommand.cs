using MediatR;

namespace SPage.Application.Features.ProductCategories.Commands;

public sealed record DeleteProductCategoryCommand(int Id) : IRequest<bool>;
