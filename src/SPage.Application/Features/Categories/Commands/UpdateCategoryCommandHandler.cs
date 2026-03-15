using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.Categories.Commands;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugLookupService _slugLookup;

    public UpdateCategoryCommandHandler(IApplicationDbContext context, ISlugLookupService slugLookup)
    {
        _context = context;
        _slugLookup = slugLookup;
    }

    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return false;

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentId = request.ParentId;
        category.MetaTitle = string.IsNullOrWhiteSpace(request.MetaTitle) ? request.Name : request.MetaTitle;
        category.MetaDescription = request.MetaDescription;
        category.CanonicalUrl = request.CanonicalUrl;
        category.IsIndexable = request.IsIndexable;
        category.ThumbnailUrl = request.ThumbnailUrl;
        category.ThumbnailAlt = request.ThumbnailAlt;
        category.BannerUrl = request.BannerUrl;
        category.BannerAlt = request.BannerAlt;

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var newSlug = request.Slug.Trim().ToLowerInvariant();
            if (!string.Equals(category.Slug, newSlug, StringComparison.OrdinalIgnoreCase))
            {
                var duplicateCat = await _context.Categories
                    .AnyAsync(c => c.Slug == newSlug && c.Id != request.Id, cancellationToken);
                if (duplicateCat) return false;
                category.Slug = newSlug;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _slugLookup.SyncCategoryAsync(category.Slug, category.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

