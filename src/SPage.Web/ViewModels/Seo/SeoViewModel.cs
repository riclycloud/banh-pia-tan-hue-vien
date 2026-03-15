namespace SPage.ViewModels.Seo;

public sealed class SeoViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool IsIndexable { get; set; } = true;
    public string Robots => IsIndexable ? "index,follow" : "noindex,follow";

    // Open Graph
    public string OgType { get; set; } = "website";
    public string? OgImage { get; set; }
    public string? OgImageAlt { get; set; }

    // Twitter Card
    public string TwitterCard { get; set; } = "summary_large_image";

    // Schema.org JSON-LD
    public string? SchemaOrgJson { get; set; }

    // i18n hreflang — key: culture code, value: full URL
    public Dictionary<string, string> HreflangUrls { get; set; } = [];
}
