using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Tags.DTOs;

namespace SPage.Application.Features.Tags.Queries;

public sealed class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, List<TagDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTagsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name, t.Slug))
            .ToListAsync(cancellationToken);
    }
}
