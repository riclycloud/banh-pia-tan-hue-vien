using MediatR;

namespace SPage.Application.Features.ProductCategories.Commands;

public sealed record CreateProductCategoryCommand(
    string Name,
    string? Description,
    int? ParentId
) : IRequest<int>;
