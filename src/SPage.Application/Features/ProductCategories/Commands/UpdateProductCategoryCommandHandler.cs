using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.ProductCategories.Commands;

public sealed class UpdateProductCategoryCommandHandler
    : IRequestHandler<UpdateProductCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugLookupService _slugLookup;

    public UpdateProductCategoryCommandHandler(IApplicationDbContext context, ISlugLookupService slugLookup)
    {
        _context = context;
        _slugLookup = slugLookup;
    }

    public async Task<bool> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null) return false;

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentId = request.ParentId;
        category.MetaTitle = string.IsNullOrWhiteSpace(request.MetaTitle) ? request.Name : request.MetaTitle;
        category.MetaDescription = request.MetaDescription;
        category.CanonicalUrl = request.CanonicalUrl;
        category.IsIndexable = request.IsIndexable;
        category.ThumbnailUrl = request.ThumbnailUrl;
        category.ThumbnailAlt = request.ThumbnailAlt;

        var newSlug = string.IsNullOrWhiteSpace(request.Slug)
            ? category.Slug
            : request.Slug.Trim().ToLowerInvariant();

        if (!string.Equals(category.Slug, newSlug, StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await _context.ProductCategories
                .AnyAsync(c => c.Slug == newSlug && c.Id != request.Id, cancellationToken);
            if (duplicate)
                return false;
            category.Slug = newSlug;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _slugLookup.SyncProductCategoryAsync(category.Slug, category.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
