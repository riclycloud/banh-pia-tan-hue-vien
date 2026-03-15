using MediatR;
using SPage.Application.Features.Categories.DTOs;

namespace SPage.Application.Features.Categories.Queries;

public sealed record GetCategoriesQuery : IRequest<List<CategoryDto>>;
