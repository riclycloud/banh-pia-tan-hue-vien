using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.ProductCategories.Commands;

public sealed class DeleteProductCategoryCommandHandler
    : IRequestHandler<DeleteProductCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugLookupService _slugLookup;

    public DeleteProductCategoryCommandHandler(IApplicationDbContext context, ISlugLookupService slugLookup)
    {
        _context = context;
        _slugLookup = slugLookup;
    }

    public async Task<bool> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null) return false;

        await _slugLookup.RemoveAsync(SlugLookupConstants.TypeProductCategory, request.Id, cancellationToken);
        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
