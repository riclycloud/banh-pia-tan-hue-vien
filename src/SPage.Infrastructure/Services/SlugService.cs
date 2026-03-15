using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SPage.Application.Common.Interfaces;

namespace SPage.Infrastructure.Services;

public sealed partial class SlugService : ISlugService
{
    public string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Normalize Unicode (NFD) — tách dấu ra khỏi ký tự
        var normalized = input.Normalize(NormalizationForm.FormD);

        // Loại bỏ dấu kết hợp (combining marks)
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        // Thay thế đ/Đ (tiếng Việt đặc biệt)
        slug = slug.Replace("đ", "d").Replace("Đ", "d");

        // Chỉ giữ lại a-z, 0-9, dấu gạch ngang
        slug = NonAlphanumericRegex().Replace(slug, "-");

        // Gộp nhiều dấu gạch ngang liên tiếp
        slug = MultipleHyphensRegex().Replace(slug, "-");

        // Trim đầu cuối
        return slug.Trim('-');
    }

    public async Task<string> GenerateUniqueAsync(string input, Func<string, Task<bool>> existsFunc)
    {
        var baseSlug = Generate(input);
        var slug = baseSlug;
        var counter = 1;

        while (await existsFunc(slug))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        return slug;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex NonAlphanumericRegex();

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex MultipleHyphensRegex();
}
