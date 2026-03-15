using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.Products.Commands;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugLookupService _slugLookup;

    public DeleteProductCommandHandler(IApplicationDbContext context, ISlugLookupService slugLookup)
    {
        _context = context;
        _slugLookup = slugLookup;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null) return false;

        await _slugLookup.RemoveAsync(SlugLookupConstants.TypeProduct, request.Id, cancellationToken);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

