using MediatR;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Blog;
using SPage.Domain.Enums;

namespace SPage.Application.Features.Posts.Commands;

public sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ISlugService _slugService;
    private readonly ISlugLookupService _slugLookup;

    public CreatePostCommandHandler(IApplicationDbContext context, ISlugService slugService, ISlugLookupService slugLookup)
    {
        _context = context;
        _slugService = slugService;
        _slugLookup = slugLookup;
    }

    public async Task<int> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var slugInput = !string.IsNullOrWhiteSpace(request.Slug) ? request.Slug : request.Title;
        var slug = await _slugService.GenerateUniqueAsync(
            slugInput,
            async s => await _context.Posts.AnyAsync(p => p.Slug == s, cancellationToken)
                || await _slugLookup.IsSlugInUseAsync(s, SlugLookupConstants.TypePost, null, cancellationToken));

        var post = new Post
        {
            Title = request.Title,
            Slug = slug,
            Content = request.Content,
            Excerpt = request.Excerpt,
            CategoryId = request.CategoryId,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            FeaturedImageUrl = request.FeaturedImageUrl,
            FeaturedImageAlt = request.FeaturedImageAlt,
            IsIndexable = request.IsIndexable,
            SchemaType = request.SchemaType,
            Status = request.Status
        };
        if (request.Status == PostStatus.Published)
            post.PublishedAt = DateTimeOffset.UtcNow;

        // Attach tags
        if (request.TagIds.Count > 0)
        {
            var tags = await _context.Tags
                .Where(t => request.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            post.PostTags = tags.Select(t => new PostTag
            {
                Post = post,
                TagId = t.Id
            }).ToList();
        }

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);

        await _slugLookup.SyncPostAsync(slug, post.Id, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return post.Id;
    }
}
