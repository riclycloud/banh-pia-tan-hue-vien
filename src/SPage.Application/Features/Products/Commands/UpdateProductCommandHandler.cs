using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.Products.Commands;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugService _slugService;
    private readonly ISlugLookupService _slugLookup;

    public UpdateProductCommandHandler(
        IApplicationDbContext context,
        ISlugService slugService,
        ISlugLookupService slugLookup)
    {
        _context = context;
        _slugService = slugService;
        _slugLookup = slugLookup;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null) return false;

        var slugInput = !string.IsNullOrWhiteSpace(request.Slug) ? request.Slug : request.Name;
        if (!string.Equals(slugInput, product.Slug, StringComparison.OrdinalIgnoreCase))
        {
            var slug = await _slugService.GenerateUniqueAsync(
                slugInput,
                async s => await _context.Products.AnyAsync(p => p.Slug == s && p.Id != request.Id, cancellationToken)
                    || await _slugLookup.IsSlugInUseAsync(s, SlugLookupConstants.TypeProduct, request.Id, cancellationToken));
            product.Slug = slug;
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.ShortDescription = request.ShortDescription;
        product.Price = request.Price;
        product.SalePrice = request.SalePrice;
        product.SKU = request.SKU;
        product.IsActive = request.IsActive;
        product.IsNew = request.IsNew;
        product.IsFeatured = request.IsFeatured;
        product.IsPromotion = request.IsPromotion;
        product.SortOrder = request.SortOrder;
        product.CategoryId = request.CategoryId;
        product.MetaTitle = string.IsNullOrWhiteSpace(request.MetaTitle) ? request.Name : request.MetaTitle;
        product.MetaDescription = request.MetaDescription;
        product.CanonicalUrl = request.CanonicalUrl;
        product.IsIndexable = request.IsIndexable;

        var primaryImage = product.Images.FirstOrDefault(i => i.IsPrimary);

        if (string.IsNullOrWhiteSpace(request.PrimaryImageUrl))
        {
            if (primaryImage is not null)
            {
                _context.ProductImages.Remove(primaryImage);
            }
        }
        else
        {
            if (primaryImage is null)
            {
                product.Images.Add(new Domain.Entities.Product.ProductImage
                {
                    ImageUrl = request.PrimaryImageUrl,
                    AltText = request.PrimaryImageAlt,
                    IsPrimary = true,
                    SortOrder = 0
                });
            }
            else
            {
                primaryImage.ImageUrl = request.PrimaryImageUrl;
                primaryImage.AltText = request.PrimaryImageAlt;
                primaryImage.IsPrimary = true;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (product.IsActive)
        {
            var catSlug = await _context.ProductCategories
                .AsNoTracking()
                .Where(c => c.Id == product.CategoryId)
                .Select(c => c.Slug)
                .FirstOrDefaultAsync(cancellationToken) ?? "san-pham";
            await _slugLookup.SyncProductAsync(product.Slug, product.Id, catSlug, cancellationToken);
        }
        else
        {
            await _slugLookup.RemoveAsync(SlugLookupConstants.TypeProduct, product.Id, cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

