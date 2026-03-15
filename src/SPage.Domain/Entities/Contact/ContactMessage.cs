using SPage.Domain.Common;

namespace SPage.Domain.Entities.Contact;

public enum ContactMessageStatus
{
    New = 0,
    InProgress = 1,
    Resolved = 2,
    Archived = 3
}

public sealed class ContactMessage : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Tiêu đề / loại form (vd: Đăng ký tham quan, Liên hệ chung) để phân biệt nguồn gửi.</summary>
    public string? Subject { get; set; }

    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public string? SenderPhone { get; set; }

    /// <summary>IP của người gửi (nếu có).</summary>
    public string? SenderIp { get; set; }

    public ContactMessageStatus Status { get; set; } = ContactMessageStatus.New;

    /// <summary>Ghi chú xử lý nội bộ.</summary>
    public string? Notes { get; set; }

    /// <summary>Dữ liệu form lưu dạng JSON key-value (fieldName → value).</summary>
    public string DataJson { get; set; } = string.Empty;
}

