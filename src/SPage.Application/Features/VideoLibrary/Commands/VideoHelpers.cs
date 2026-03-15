using SPage.Domain.Enums;

namespace SPage.Application.Features.VideoLibrary.Commands;

/// <summary>Helpers dùng chung trong các Command handlers</summary>
internal static class VideoHelpers
{
    /// <summary>Tự động phát hiện platform từ domain của URL</summary>
    public static VideoPlatform AutoDetectPlatform(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return VideoPlatform.YouTube;
        var lower = url.ToLowerInvariant();
        if (lower.Contains("youtube.com") || lower.Contains("youtu.be"))  return VideoPlatform.YouTube;
        if (lower.Contains("facebook.com") || lower.Contains("fb.watch")) return VideoPlatform.Facebook;
        if (lower.Contains("tiktok.com"))                                  return VideoPlatform.TikTok;
        return VideoPlatform.YouTube;
    }

    /// <summary>Trích xuất VideoId từ URL theo từng platform</summary>
    public static string ExtractVideoId(VideoPlatform platform, string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;

        try
        {
            switch (platform)
            {
                case VideoPlatform.YouTube:
                    // youtu.be/ID
                    if (url.Contains("youtu.be/"))
                    {
                        var seg = url.Split("youtu.be/")[1].Split('?')[0].Split('/')[0];
                        if (!string.IsNullOrEmpty(seg)) return seg;
                    }
                    // /shorts/ID
                    if (url.Contains("/shorts/"))
                    {
                        var seg = url.Split("/shorts/")[1].Split('?')[0].Split('/')[0];
                        if (!string.IsNullOrEmpty(seg)) return seg;
                    }
                    // v= param
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    {
                        var qs = System.Web.HttpUtility.ParseQueryString(uri.Query);
                        var v  = qs["v"];
                        if (!string.IsNullOrEmpty(v)) return v;
                    }
                    break;

                case VideoPlatform.TikTok:
                    // /@user/video/ID
                    if (url.Contains("/video/"))
                    {
                        var seg = url.Split("/video/")[1].Split('?')[0].Split('/')[0];
                        if (!string.IsNullOrEmpty(seg)) return seg;
                    }
                    break;

                case VideoPlatform.Facebook:
                    // videos/ID or video_id param
                    if (url.Contains("/videos/"))
                    {
                        var seg = url.Split("/videos/")[1].TrimEnd('/').Split('?')[0];
                        if (!string.IsNullOrEmpty(seg)) return seg;
                    }
                    if (Uri.TryCreate(url, UriKind.Absolute, out var fbUri))
                    {
                        var qs = System.Web.HttpUtility.ParseQueryString(fbUri.Query);
                        var vid = qs["video_id"] ?? qs["v"];
                        if (!string.IsNullOrEmpty(vid)) return vid;
                    }
                    break;
            }
        }
        catch { /* ignore parsing errors */ }

        return url; // fallback: dùng nguyên URL
    }
}
