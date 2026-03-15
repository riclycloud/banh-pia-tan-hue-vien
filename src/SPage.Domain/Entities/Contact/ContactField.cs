using SPage.Domain.Common;

namespace SPage.Domain.Entities.Contact;

public enum ContactFieldType
{
    Text = 0,
    TextArea = 1,
    Email = 2,
    Phone = 3,
    Number = 4
}

public sealed class ContactField : BaseEntity
{
    /// <summary>Khóa hệ thống, dùng làm name trong form (vd: fullName, phone, address, message)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Nhãn hiển thị cho người dùng</summary>
    public string Label { get; set; } = string.Empty;

    public string? Placeholder { get; set; }

    public ContactFieldType FieldType { get; set; } = ContactFieldType.Text;

    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;
}

