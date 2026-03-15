namespace SPage.Domain.Entities.Email;

/// <summary>
/// Cấu hình SMTP / email gửi đi (một bản ghi duy nhất, Id = 1).
/// Admin có thể sửa trong trang Quản lý email.
/// </summary>
public sealed class EmailSendConfig
{
    public int Id { get; set; }

    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;

    public string FromEmail { get; set; } = string.Empty;
    public string? FromName { get; set; }

    /// <summary>Để trống nếu SMTP không cần đăng nhập.</summary>
    public string? UserName { get; set; }

    /// <summary>Mật khẩu SMTP (lưu dạng plain trong DB; chỉ Admin xem được).</summary>
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
}
