using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace SPage.Controllers;

/// <summary>
/// Controller xử lý chuyển đổi ngôn ngữ.
/// Lưu culture vào cookie và redirect về trang gốc.
/// </summary>
public sealed class LanguageController : Controller
{
    private static readonly HashSet<string> SupportedCultures = ["vi", "en"];

    [HttpGet("language/set")]
    public IActionResult SetLanguage(string culture, string? returnUrl = null)
    {
        if (!SupportedCultures.Contains(culture))
            culture = "vi";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                HttpOnly = false // Cần accessible từ JavaScript nếu cần
            });

        // Redirect an toàn — tránh open redirect
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
