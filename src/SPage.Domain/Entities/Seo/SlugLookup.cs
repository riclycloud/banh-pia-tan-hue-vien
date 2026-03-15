namespace SPage.Domain.Entities.Seo;

/// <summary>
/// Bảng tra cứu slug → rewrite (1 query thay vì tra nhiều bảng).
/// EntityType: product | productcategory | category | post
/// </summary>
public sealed class SlugLookup
{
    public int Id { get; set; }

    /// <summary>Slug (lowercase, unique)</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>product | productcategory | category | post</summary>
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    /// <summary>Path rewrite, VD: /Products/Detail</summary>
    public string RewritePath { get; set; } = string.Empty;

    /// <summary>Query string rewrite, VD: categorySlug=x&amp;productSlug=y</summary>
    public string RewriteQuery { get; set; } = string.Empty;
}
