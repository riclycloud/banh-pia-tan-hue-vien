using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Blog;

namespace SPage.Application.Features.Categories.Commands;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugService _slugService;
    private readonly ISlugLookupService _slugLookup;

    public CreateCategoryCommandHandler(IApplicationDbContext context, ISlugService slugService, ISlugLookupService slugLookup)
    {
        _context = context;
        _slugService = slugService;
        _slugLookup = slugLookup;
    }

    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var slug = _slugService.Generate(request.Name);

        var category = new Category
        {
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            ParentId = request.ParentId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        await _slugLookup.SyncCategoryAsync(slug, category.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
