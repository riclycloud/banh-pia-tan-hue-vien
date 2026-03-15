using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;

namespace SPage.Application.Features.VideoLibrary.Commands;

internal sealed class UpdateVideoCommandHandler : IRequestHandler<UpdateVideoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ISlugService          _slugService;

    public UpdateVideoCommandHandler(IApplicationDbContext db, ISlugService slugService)
    {
        _db          = db;
        _slugService = slugService;
    }

    public async Task Handle(UpdateVideoCommand req, CancellationToken ct)
    {
        var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == req.Id, ct)
            ?? throw new InvalidOperationException($"Video {req.Id} không tồn tại.");

        // Re-generate slug only if title or explicit slug changed
        var slugInput = !string.IsNullOrWhiteSpace(req.Slug) ? req.Slug : req.Title;
        var newSlug   = await _slugService.GenerateUniqueAsync(
            slugInput,
            async s => s != video.Slug && await _db.Videos.AnyAsync(v => v.Slug == s, ct));

        // If video just published for first time
        var publishedAt = video.PublishedAt;
        if (req.IsPublished && publishedAt is null)
            publishedAt = DateTimeOffset.UtcNow;
        else if (!req.IsPublished)
            publishedAt = null;

        video.Title           = req.Title.Trim();
        video.Slug            = newSlug;
        video.Description     = req.Description?.Trim();
        video.Platform        = req.Platform;
        video.VideoId         = req.VideoId.Trim();
        video.VideoUrl        = req.VideoUrl.Trim();
        video.ThumbnailUrl    = string.IsNullOrWhiteSpace(req.ThumbnailUrl) ? null : req.ThumbnailUrl.Trim();
        video.Duration        = req.Duration?.Trim();
        video.CategoryId      = req.CategoryId;
        video.TagsCsv         = req.TagsCsv?.Trim();
        video.IsPublished     = req.IsPublished;
        video.PublishedAt     = publishedAt;
        video.IsFeatured      = req.IsFeatured;
        video.MetaTitle       = req.MetaTitle?.Trim();
        video.MetaDescription = req.MetaDescription?.Trim();
        video.IsIndexable     = req.IsIndexable;

        await _db.SaveChangesAsync(ct);
    }
}
