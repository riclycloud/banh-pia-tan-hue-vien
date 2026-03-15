using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.VideoLibrary.Commands;

internal sealed class UpdateVideoCategoryCommandHandler : IRequestHandler<UpdateVideoCategoryCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ISlugService          _slugService;

    public UpdateVideoCategoryCommandHandler(IApplicationDbContext db, ISlugService slugService)
    {
        _db          = db;
        _slugService = slugService;
    }

    public async Task Handle(UpdateVideoCategoryCommand req, CancellationToken ct)
    {
        var cat = await _db.VideoCategories.FirstOrDefaultAsync(c => c.Id == req.Id, ct)
            ?? throw new InvalidOperationException($"VideoCategory {req.Id} không tồn tại.");

        var slug = await _slugService.GenerateUniqueAsync(
            !string.IsNullOrWhiteSpace(req.Slug) ? req.Slug : req.Name,
            async s => s != cat.Slug && await _db.VideoCategories.AnyAsync(c => c.Slug == s, ct));

        cat.Name         = req.Name.Trim();
        cat.Slug         = slug;
        cat.Description  = req.Description?.Trim();
        cat.DisplayOrder = req.DisplayOrder;
        cat.IsActive     = req.IsActive;

        await _db.SaveChangesAsync(ct);
    }
}
