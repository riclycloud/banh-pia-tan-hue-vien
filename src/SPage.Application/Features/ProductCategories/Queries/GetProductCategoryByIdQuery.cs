using MediatR;
using SPage.Application.Features.ProductCategories.DTOs;

namespace SPage.Application.Features.ProductCategories.Queries;

public sealed record GetProductCategoryByIdQuery(int Id) : IRequest<ProductCategoryDetailDto?>;
