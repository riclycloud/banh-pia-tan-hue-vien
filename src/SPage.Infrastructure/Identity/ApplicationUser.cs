using Microsoft.AspNetCore.Identity;

namespace SPage.Infrastructure.Identity;

/// <summary>
/// ApplicationUser thuộc Infrastructure vì phụ thuộc Microsoft.AspNetCore.Identity (framework concern).
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsActive { get; set; } = true;
    /// <summary>Ngôn ngữ ưa thích của user: "vi" | "en"</summary>
    public string PreferredLanguage { get; set; } = "vi";
}
