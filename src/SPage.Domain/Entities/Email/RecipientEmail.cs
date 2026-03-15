namespace SPage.Domain.Entities.Email;

/// <summary>
/// Danh sách email có thể nhận (thông báo, form liên hệ, bản tin...).
/// Admin quản lý CRUD trong trang Quản lý email.
/// </summary>
public sealed class RecipientEmail
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }

    /// <summary>Nhóm/mục đích: Contact, Newsletter, Notification...</summary>
    public string? GroupKey { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
