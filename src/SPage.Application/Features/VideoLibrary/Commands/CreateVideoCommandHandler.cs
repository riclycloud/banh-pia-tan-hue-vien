using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Video;

namespace SPage.Application.Features.VideoLibrary.Commands;

internal sealed class CreateVideoCommandHandler : IRequestHandler<CreateVideoCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly ISlugService          _slugService;

    public CreateVideoCommandHandler(IApplicationDbContext db, ISlugService slugService)
    {
        _db          = db;
        _slugService = slugService;
    }

    public async Task<int> Handle(CreateVideoCommand req, CancellationToken ct)
    {
        // Detect platform if not provided
        var platform = req.Platform ?? VideoHelpers.AutoDetectPlatform(req.VideoUrl);

        // Extract VideoId if not provided
        var videoId = !string.IsNullOrWhiteSpace(req.VideoId)
            ? req.VideoId.Trim()
            : VideoHelpers.ExtractVideoId(platform, req.VideoUrl);

        // Generate unique slug
        var slug = await _slugService.GenerateUniqueAsync(
            !string.IsNullOrWhiteSpace(req.Slug) ? req.Slug : req.Title,
            async s => await _db.Videos.AnyAsync(v => v.Slug == s, ct));

        var video = new VideoEntity
        {
            Title          = req.Title.Trim(),
            Slug           = slug,
            Description    = req.Description?.Trim(),
            Platform       = platform,
            VideoId        = videoId,
            VideoUrl       = req.VideoUrl.Trim(),
            ThumbnailUrl   = string.IsNullOrWhiteSpace(req.ThumbnailUrl) ? null : req.ThumbnailUrl.Trim(),
            Duration       = req.Duration?.Trim(),
            CategoryId     = req.CategoryId,
            TagsCsv        = req.TagsCsv?.Trim(),
            IsPublished    = req.IsPublished,
            PublishedAt    = req.IsPublished ? DateTimeOffset.UtcNow : null,
            IsFeatured     = req.IsFeatured,
            MetaTitle      = req.MetaTitle?.Trim(),
            MetaDescription= req.MetaDescription?.Trim(),
            IsIndexable    = req.IsIndexable
        };

        _db.Videos.Add(video);
        await _db.SaveChangesAsync(ct);
        return video.Id;
    }
}
