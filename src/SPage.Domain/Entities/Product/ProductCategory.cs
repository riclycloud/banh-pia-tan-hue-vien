using SPage.Domain.Common;

namespace SPage.Domain.Entities.Product;

public sealed class ProductCategory : BaseAuditableEntity, ISeoEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool IsIndexable { get; set; } = true;
    public string? Description { get; set; }
    public int? ParentId { get; set; }
    public ProductCategory? Parent { get; set; }
    public ICollection<ProductCategory> Children { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
    public string? ThumbnailUrl { get; set; }
    public string? ThumbnailAlt { get; set; }
}
