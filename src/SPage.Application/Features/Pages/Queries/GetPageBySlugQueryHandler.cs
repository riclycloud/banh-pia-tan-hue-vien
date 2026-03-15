using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Application.Features.Pages.DTOs;
using SPage.Domain.Enums;

namespace SPage.Application.Features.Pages.Queries;

public sealed class GetPageBySlugQueryHandler
    : IRequestHandler<GetPageBySlugQuery, DynamicPageDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPageBySlugQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<DynamicPageDto?> Handle(
        GetPageBySlugQuery request,
        CancellationToken cancellationToken)
    {
        // Load entity trước để EF materialize đầy đủ cột JSON Sections (kể cả Data raw HTML),
        // rồi map sang DTO trong memory. Projection .Select() đôi khi không deserialize hết Data.
        var page = await _context.DynamicPages
            .AsNoTracking()
            .Where(p => p.Slug == request.Slug && p.Status == PageStatus.Published)
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
