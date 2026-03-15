using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Blog;
using SPage.Domain.Enums;

namespace SPage.Application.Features.Posts.Commands;

public sealed class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugService _slugService;
    private readonly ISlugHistoryService _slugHistory;
    private readonly ISlugLookupService _slugLookup;

    public UpdatePostCommandHandler(
        IApplicationDbContext context,
        ISlugService slugService,
        ISlugHistoryService slugHistory,
        ISlugLookupService slugLookup)
    {
        _context = context;
        _slugService = slugService;
        _slugHistory = slugHistory;
        _slugLookup = slugLookup;
    }

    public async Task<bool> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .Include(p => p.PostTags)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post is null) return false;

        var oldSlug = post.Slug;
        // Dùng slug do admin cung cấp, hoặc tự sinh từ tiêu đề nếu bỏ trống
        var slugInput = !string.IsNullOrWhiteSpace(request.Slug) ? request.Slug : request.Title;
        var newSlug = _slugService.Generate(slugInput);

        // Nếu slug thay đổi, lưu vào lịch sử để redirect 301
        if (!oldSlug.Equals(newSlug, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrEmpty(oldSlug))
        {
            await _slugHistory.RecordAsync(
                oldSlug, newSlug,
                entityType: "post",
                entityId: post.Id,
                redirectPath: $"/{newSlug}",
                cancellationToken: cancellationToken);
        }

        post.Title = request.Title;
        post.Slug = newSlug;
        post.Content = request.Content;
        post.Excerpt = request.Excerpt;
        post.CategoryId = request.CategoryId;
        post.MetaTitle = request.MetaTitle;
        post.MetaDescription = request.MetaDescription;
        post.CanonicalUrl = request.CanonicalUrl;
        post.FeaturedImageUrl = request.FeaturedImageUrl;
        post.FeaturedImageAlt = request.FeaturedImageAlt;
        post.IsIndexable = request.IsIndexable;
        post.SchemaType = request.SchemaType;

        // Publish nếu status chuyển sang Published
        if (request.Status == PostStatus.Published && post.Status != PostStatus.Published)
            post.Publish();
        else
            post.Status = request.Status;

        // Cập nhật tags
        post.PostTags.Clear();
        foreach (var tagId in request.TagIds)
            post.PostTags.Add(new PostTag { PostId = post.Id, TagId = tagId });

        await _context.SaveChangesAsync(cancellationToken);

        await _slugLookup.SyncPostAsync(newSlug, post.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
