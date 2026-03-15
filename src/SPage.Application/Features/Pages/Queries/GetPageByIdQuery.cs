using MediatR;
using SPage.Application.Features.Pages.DTOs;

namespace SPage.Application.Features.Pages.Queries;

public sealed record GetPageByIdQuery(int Id) : IRequest<DynamicPageDto?>;
