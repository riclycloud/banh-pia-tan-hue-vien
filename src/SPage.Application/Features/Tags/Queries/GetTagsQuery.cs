using MediatR;
using SPage.Application.Features.Tags.DTOs;

namespace SPage.Application.Features.Tags.Queries;

public sealed record GetTagsQuery : IRequest<List<TagDto>>;
