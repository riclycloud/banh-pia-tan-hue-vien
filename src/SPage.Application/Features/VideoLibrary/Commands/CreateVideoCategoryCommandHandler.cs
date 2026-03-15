using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Video;

namespace SPage.Application.Features.VideoLibrary.Commands;

internal sealed class CreateVideoCategoryCommandHandler
    : IRequestHandler<CreateVideoCategoryCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ISlugService          _slugService;

    public CreateVideoCategoryCommandHandler(IApplicationDbContext db, ISlugService slugService)
    {
        _db          = db;
        _slugService = slugService;
    }

    public async Task<int> Handle(CreateVideoCategoryCommand req, CancellationToken ct)
    {
        var slug = await _slugService.GenerateUniqueAsync(
            !string.IsNullOrWhiteSpace(req.Slug) ? req.Slug : req.Name,
            async s => await _db.VideoCategories.AnyAsync(c => c.Slug == s, ct));

        var cat = new VideoCategory
        {
            Name         = req.Name.Trim(),
            Slug         = slug,
            Description  = req.Description?.Trim(),
            DisplayOrder = req.DisplayOrder,
            IsActive     = req.IsActive
        };

        _db.VideoCategories.Add(cat);
        await _db.SaveChangesAsync(ct);
        return cat.Id;
    }
}
