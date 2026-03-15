using SPage.Domain.Common;

namespace SPage.Domain.Entities.Blog;

public sealed class Category : BaseAuditableEntity, ISeoEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool IsIndexable { get; set; } = true;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public ICollection<Post> Posts { get; set; } = [];

    public string? ThumbnailUrl { get; set; }
    public string? ThumbnailAlt { get; set; }
    public string? BannerUrl { get; set; }
    public string? BannerAlt { get; set; }
}
