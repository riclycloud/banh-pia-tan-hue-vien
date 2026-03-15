using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Product;

namespace SPage.Application.Features.ProductCategories.Commands;

public sealed class CreateProductCategoryCommandHandler
    : IRequestHandler<CreateProductCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugService _slugService;
    private readonly ISlugLookupService _slugLookup;

    public CreateProductCategoryCommandHandler(
        IApplicationDbContext context,
        ISlugService slugService,
        ISlugLookupService slugLookup)
    {
        _context = context;
        _slugService = slugService;
        _slugLookup = slugLookup;
    }

    public async Task<int> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var slug = await _slugService.GenerateUniqueAsync(
            request.Name,
            async s => await _context.ProductCategories.AnyAsync(c => c.Slug == s, cancellationToken)
                || await _slugLookup.IsSlugInUseAsync(s, SlugLookupConstants.TypeProductCategory, null, cancellationToken));

        var category = new ProductCategory
        {
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            ParentId = request.ParentId,
            MetaTitle = request.Name,
            IsIndexable = true
        };

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        await _slugLookup.SyncProductCategoryAsync(slug, category.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
