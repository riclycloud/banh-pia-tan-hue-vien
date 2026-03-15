using SPage.Domain.Common;
using SPage.Domain.Enums;
using SPage.Domain.Events;

namespace SPage.Domain.Entities.Blog;

public sealed class Post : BaseAuditableEntity, ISeoEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool IsIndexable { get; set; } = true;

    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? FeaturedImageAlt { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }
    public int ViewCount { get; set; } = 0;

    // Schema.org: Article | NewsArticle | BlogPosting
    public string SchemaType { get; set; } = "Article";

    // Relationships
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<PostTag> PostTags { get; set; } = [];

    // Domain method
    public void Publish()
    {
        if (Status == PostStatus.Draft || Status == PostStatus.Scheduled)
        {
            Status = PostStatus.Published;
            PublishedAt = DateTimeOffset.UtcNow;
            AddDomainEvent(new PostPublishedEvent(this));
        }
    }
}
