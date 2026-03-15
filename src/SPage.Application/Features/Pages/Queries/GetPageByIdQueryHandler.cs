using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Pages.DTOs;
using SPage.Domain.Enums;

namespace SPage.Application.Features.Pages.Queries;

public sealed class GetPageByIdQueryHandler : IRequestHandler<GetPageByIdQuery, DynamicPageDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPageByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<DynamicPageDto?> Handle(
        GetPageByIdQuery request,
        CancellationToken cancellationToken)
    {
        var page = await _context.DynamicPages
            .AsNoTracking()
            .Where(p => p.Id == request.Id && p.Status == PageStatus.Published)
            .FirstOrDefaultAsync(cancellationToken);

        if (page is null) return null;

        var sections = page.Sections
            .Where(s => s.IsVisible)
            .OrderBy(s => s.SortOrder)
            .ToList();

        return new DynamicPageDto(
            page.Id,
            page.Title,
            page.Slug,
            page.Template,
            sections,
            page.MetaTitle ?? page.Title,
            page.MetaDescription,
            page.CanonicalUrl,
            page.IsIndexable
        );
    }
}
