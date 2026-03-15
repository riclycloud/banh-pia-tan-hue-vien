using SPage.Domain.Entities.Email;

namespace SPage.Application.Common.Interfaces;

/// <summary>
/// Đọc cấu hình email gửi từ DB (nếu có). Dùng cho SmtpEmailService khi gửi mail.
/// </summary>
public interface IEmailConfigService
{
    /// <summary>Lấy bản ghi cấu hình gửi (Id=1). Null nếu chưa cấu hình.</summary>
    Task<EmailSendConfig?> GetSendConfigAsync(CancellationToken cancellationToken = default);

    /// <summary>Lấy danh sách email nhận đang bật, có thể lọc theo GroupKey.</summary>
    Task<IReadOnlyList<RecipientEmail>> GetActiveRecipientsAsync(string? groupKey = null, CancellationToken cancellationToken = default);
}
