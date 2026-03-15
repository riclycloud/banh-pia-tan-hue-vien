using MediatR;
using SPage.Application.Features.Products.DTOs;

namespace SPage.Application.Features.Products.Queries;

public sealed record GetProductBySlugQuery(string Slug) : IRequest<ProductDetailDto?>;
