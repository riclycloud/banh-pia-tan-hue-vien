using MediatR;
using SPage.Application.Features.Categories.DTOs;

namespace SPage.Application.Features.Categories.Queries;

public sealed record GetCategoryByIdQuery(int Id) : IRequest<CategoryDetailDto?>;

