using Microsoft.EntityFrameworkCore;
using SPage.Domain.Entities.Contact;
using SPage.Infrastructure.Persistence;

namespace SPage.Web.SeedData;

internal static partial class DataSeeder
{
    /// <summary>
    /// Seed ContactField cho form đăng ký tham quan.
    /// Luôn xóa + tạo lại để cập nhật định nghĩa field khi cần.
    /// </summary>
    public static async Task SeedContactFieldsAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<ApplicationDbContext>();

        var all = await context.ContactFields.ToListAsync();
        if (all.Count > 0)
        {
            context.ContactFields.RemoveRange(all);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var fields = new List<ContactField>
        {
            new()
            {
                Name        = "hoTen",
                Label       = "Họ và tên",
                Placeholder = "Nhập họ và tên đầy đủ",
                FieldType   = ContactFieldType.Text,
                IsRequired  = true,
                IsActive    = true,
                SortOrder   = 1,
            },
            new()
            {
                Name        = "phone",
                Label       = "Số điện thoại",
                Placeholder = "Ví dụ: 0901 234 567",
                FieldType   = ContactFieldType.Phone,
                IsRequired  = true,
                IsActive    = true,
                SortOrder   = 2,
            },
            new()
            {
                Name        = "email",
                Label       = "Email",
                Placeholder = "email@example.com",
                FieldType   = ContactFieldType.Email,
                IsRequired  = false,
                IsActive    = true,
                SortOrder   = 3,
            },
            new()
            {
                Name        = "soLuong",
                Label       = "Số lượng người tham quan",
                Placeholder = "Ví dụ: 10",
                FieldType   = ContactFieldType.Number,
                IsRequired  = true,
                IsActive    = true,
                SortOrder   = 4,
            },
            new()
            {
                Name        = "ngayDuKien",
                Label       = "Ngày dự kiến tham quan",
                Placeholder = "Ví dụ: 20/06/2025",
                FieldType   = ContactFieldType.Text,
                IsRequired  = true,
                IsActive    = true,
                SortOrder   = 5,
            },
            new()
            {
                Name        = "ghiChu",
                Label       = "Ghi chú / Yêu cầu đặc biệt",
                Placeholder = "Nhập yêu cầu nếu có (không bắt buộc)",
                FieldType   = ContactFieldType.TextArea,
                IsRequired  = false,
                IsActive    = true,
                SortOrder   = 6,
            },
        };

        context.ContactFields.AddRange(fields);
        await context.SaveChangesAsync(CancellationToken.None);
    }
}
