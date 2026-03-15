namespace SPage.Domain.Common;

public interface ISeoEntity
{
    string Slug { get; set; }
    string? MetaTitle { get; set; }
    string? MetaDescription { get; set; }
    string? CanonicalUrl { get; set; }
    bool IsIndexable { get; set; }
}
