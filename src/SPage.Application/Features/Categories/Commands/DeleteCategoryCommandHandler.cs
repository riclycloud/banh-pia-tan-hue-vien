using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.Categories.Commands;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugLookupService _slugLookup;

    public DeleteCategoryCommandHandler(IApplicationDbContext context, ISlugLookupService slugLookup)
    {
        _context = context;
        _slugLookup = slugLookup;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null) return false;

        await _slugLookup.RemoveAsync(SlugLookupConstants.TypeCategory, request.Id, cancellationToken);
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
