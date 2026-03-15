using SPage.Domain.Entities.Seo;

namespace SPage.Application.Common.Interfaces;

public interface ISiteSettingsService
{
    /// <summary>Lấy cấu hình site (tự tạo bản ghi mặc định nếu chưa có).</summary>
    Task<SiteSettings> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Cập nhật và làm mới cache.</summary>
    Task UpdateAsync(SiteSettings settings, CancellationToken cancellationToken = default);
}

