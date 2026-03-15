using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Behaviours;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.Contact.Queries;

public sealed record ContactFieldApiDto(
    string Name,
    string Label,
    string? Placeholder,
    string FieldType,
    bool IsRequired
);

public sealed record GetContactFieldsQuery : IRequest<List<ContactFieldApiDto>>, ICachedQuery
{
    public string CacheKey => "contact:fields";
    public TimeSpan? CacheDuration => TimeSpan.FromHours(1);
}

public sealed class GetContactFieldsQueryHandler
    : IRequestHandler<GetContactFieldsQuery, List<ContactFieldApiDto>>
{
    private readonly IApplicationDbContext _db;

    public GetContactFieldsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<ContactFieldApiDto>> Handle(
        GetContactFieldsQuery request, CancellationToken cancellationToken)
    {
        return await _db.ContactFields
            .AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.SortOrder)
            .Select(f => new ContactFieldApiDto(
                f.Name,
                f.Label,
                f.Placeholder,
                f.FieldType.ToString().ToLower(),
                f.IsRequired))
            .ToListAsync(cancellationToken);
    }
}
