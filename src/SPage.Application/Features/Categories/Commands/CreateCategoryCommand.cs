using MediatR;

namespace SPage.Application.Features.Categories.Commands;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description,
    int? ParentId
) : IRequest<int>;
