using MediatR;
using SPage.Application.Features.ProductCategories.DTOs;

namespace SPage.Application.Features.ProductCategories.Queries;

public sealed record GetProductCategoriesQuery : IRequest<List<ProductCategoryDto>>;
