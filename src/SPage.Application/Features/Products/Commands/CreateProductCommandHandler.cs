using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Product;

namespace SPage.Application.Features.Products.Commands;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugService _slugService;
    private readonly ISlugLookupService _slugLookup;

    public CreateProductCommandHandler(
        IApplicationDbContext context,
        ISlugService slugService,
        ISlugLookupService slugLookup)
    {
        _context = context;
        _slugService = slugService;
        _slugLookup = slugLookup;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var slugInput = !string.IsNullOrWhiteSpace(request.Slug) ? request.Slug : request.Name;
        var slug = await _slugService.GenerateUniqueAsync(
            slugInput,
            async s => await _context.Products.AnyAsync(p => p.Slug == s, cancellationToken)
                || await _slugLookup.IsSlugInUseAsync(s, SlugLookupConstants.TypeProduct, null, cancellationToken));

        var product = new Product
        {
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            Price = request.Price,
            SalePrice = request.SalePrice,
            SKU = request.SKU,
            IsActive = request.IsActive,
            IsNew = request.IsNew,
            IsFeatured = request.IsFeatured,
            IsPromotion = request.IsPromotion,
            SortOrder = request.SortOrder,
            CategoryId = request.CategoryId,
            MetaTitle = string.IsNullOrWhiteSpace(request.MetaTitle) ? request.Name : request.MetaTitle,
            MetaDescription = request.MetaDescription,
            CanonicalUrl = request.CanonicalUrl,
            IsIndexable = request.IsIndexable
        };

        if (!string.IsNullOrWhiteSpace(request.PrimaryImageUrl))
        {
            product.Images.Add(new ProductImage
            {
                ImageUrl = request.PrimaryImageUrl,
                AltText = request.PrimaryImageAlt,
                IsPrimary = true,
                SortOrder = 0
            });
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        if (product.IsActive)
        {
            var catSlug = await _context.ProductCategories
                .AsNoTracking()
                .Where(c => c.Id == product.CategoryId)
                .Select(c => c.Slug)
                .FirstOrDefaultAsync(cancellationToken) ?? "san-pham";
            await _slugLookup.SyncProductAsync(slug, product.Id, catSlug, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return product.Id;
    }
}

