using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SPage.ViewModels.Seo;

namespace SPage.TagHelpers;

/// <summary>
/// Usage trong Layout: &lt;seo-head /&gt;
/// Tự động render meta tags, og, canonical, hreflang, JSON-LD
/// </summary>
[HtmlTargetElement("seo-head")]
public sealed class SeoHeadTagHelper : TagHelper
{
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    private SeoViewModel? Seo => ViewContext.ViewData["Seo"] as SeoViewModel;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null; // Render nội dung inline, không có tag bọc ngoài

        if (Seo is null) return;

        var html = new System.Text.StringBuilder();

        // ── Title & Description ───────────────────────────────────
        html.AppendLine($"<title>{HtmlEncode(Seo.Title)}</title>");
        if (!string.IsNullOrEmpty(Seo.MetaDescription))
            html.AppendLine($"""<meta name="description" content="{HtmlEncode(Seo.MetaDescription)}" />""");
        html.AppendLine($"""<meta name="robots" content="{Seo.Robots}" />""");

        // ── Canonical ─────────────────────────────────────────────
        if (!string.IsNullOrEmpty(Seo.CanonicalUrl))
            html.AppendLine($"""<link rel="canonical" href="{Seo.CanonicalUrl}" />""");

        // ── Hreflang (đa ngôn ngữ) ────────────────────────────────
        foreach (var (lang, url) in Seo.HreflangUrls)
            html.AppendLine($"""<link rel="alternate" hreflang="{lang}" href="{url}" />""");

        // ── Open Graph ────────────────────────────────────────────
        html.AppendLine($"""<meta property="og:title" content="{HtmlEncode(Seo.Title)}" />""");
        html.AppendLine($"""<meta property="og:type" content="{Seo.OgType}" />""");
        if (!string.IsNullOrEmpty(Seo.CanonicalUrl))
            html.AppendLine($"""<meta property="og:url" content="{Seo.CanonicalUrl}" />""");
        if (!string.IsNullOrEmpty(Seo.MetaDescription))
            html.AppendLine($"""<meta property="og:description" content="{HtmlEncode(Seo.MetaDescription)}" />""");
        if (!string.IsNullOrEmpty(Seo.OgImage))
        {
            html.AppendLine($"""<meta property="og:image" content="{Seo.OgImage}" />""");
            if (!string.IsNullOrEmpty(Seo.OgImageAlt))
                html.AppendLine($"""<meta property="og:image:alt" content="{HtmlEncode(Seo.OgImageAlt)}" />""");
        }

        // ── Twitter Card ──────────────────────────────────────────
        html.AppendLine($"""<meta name="twitter:card" content="{Seo.TwitterCard}" />""");
        html.AppendLine($"""<meta name="twitter:title" content="{HtmlEncode(Seo.Title)}" />""");
        if (!string.IsNullOrEmpty(Seo.MetaDescription))
            html.AppendLine($"""<meta name="twitter:description" content="{HtmlEncode(Seo.MetaDescription)}" />""");
        if (!string.IsNullOrEmpty(Seo.OgImage))
            html.AppendLine($"""<meta name="twitter:image" content="{Seo.OgImage}" />""");

        // ── Schema.org JSON-LD ────────────────────────────────────
        if (!string.IsNullOrEmpty(Seo.SchemaOrgJson))
        {
            html.AppendLine("""<script type="application/ld+json">""");
            html.AppendLine(Seo.SchemaOrgJson);
            html.AppendLine("</script>");
        }

        output.Content.SetHtmlContent(html.ToString());
    }

    private static string HtmlEncode(string value)
        => System.Web.HttpUtility.HtmlAttributeEncode(value);
}
