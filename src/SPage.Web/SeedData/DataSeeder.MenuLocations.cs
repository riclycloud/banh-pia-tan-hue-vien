using Microsoft.EntityFrameworkCore;
using SPage.Domain.Entities.Navigation;
using SPage.Infrastructure.Persistence;

namespace SPage.Web.SeedData;

internal static partial class DataSeeder
{
    /// <summary>
    /// Seed MenuLocation mặc định (idempotent theo Key).
    /// Seed MenuItem nếu chưa có.
    /// </summary>
    public static async Task SeedMenuLocationsAsync(IServiceProvider services)
    {
        using var scope   = services.CreateScope();
        var context       = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // ── Idempotent: chỉ thêm location key chưa tồn tại ──────────────────
        var existingKeys = await context.MenuLocations.Select(l => l.Key).ToListAsync();

        var defaults = new[]
        {
            new MenuLocation { Name = "Top",    Key = "top",    SortOrder = 0, IsActive = true },
            new MenuLocation { Name = "Main",   Key = "main",   SortOrder = 1, IsActive = true },
            new MenuLocation { Name = "Footer", Key = "footer", SortOrder = 2, IsActive = true },
        };

        var toAdd = defaults.Where(d => !existingKeys.Contains(d.Key)).ToList();
        if (toAdd.Count > 0)
        {
            context.MenuLocations.AddRange(toAdd);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        // Seed menu items nếu chưa có
        if (await context.MenuItems.AnyAsync()) return;

        var locTop    = await context.MenuLocations.FirstAsync(l => l.Key == "top");
        var locMain   = await context.MenuLocations.FirstAsync(l => l.Key == "main");
        var locFooter = await context.MenuLocations.FirstAsync(l => l.Key == "footer");

        // ── PASS 1: Menu cha ─────────────────────────────────────────────────
        // Top
        var mnuVeTHV       = new MenuItem { LocationId = locTop.Id,    Title = "Về Tân Huê Viên",               Url = "/ve-tan-hue-vien",       SortOrder = 0, IsActive = true };
        var mnuThamQuan    = new MenuItem { LocationId = locTop.Id,    Title = "Tham quan Nhà máy và Liên Hoa", Url = "/tham-quan-nha-may",     SortOrder = 1, IsActive = true };
        var mnuHoTro       = new MenuItem { LocationId = locTop.Id,    Title = "Trung tâm hỗ trợ",              Url = "/trung-tam-ho-tro",      SortOrder = 2, IsActive = true };
        var mnuTuyenDung   = new MenuItem { LocationId = locTop.Id,    Title = "Tuyển dụng",                    Url = "/tuyen-dung",            SortOrder = 3, IsActive = true };

        // Main
        var mnuSanPham     = new MenuItem { LocationId = locMain.Id,   Title = "Sản phẩm",                 Url = "/san-pham",              SortOrder = 0, IsActive = true };
        var mnuNangLuc     = new MenuItem { LocationId = locMain.Id,   Title = "Năng lực sản xuất",        Url = "/nang-luc-san-xuat",     SortOrder = 1, IsActive = true };
        var mnuTruyenThong = new MenuItem { LocationId = locMain.Id,   Title = "Truyền thông",             Url = "/truyen-thong",          SortOrder = 2, IsActive = true };
        var mnuTrachNhiem  = new MenuItem { LocationId = locMain.Id,   Title = "Trách nhiệm cộng đồng",    Url = "/trach-nhiem-cong-dong", SortOrder = 3, IsActive = true };
        var mnuVuonXa      = new MenuItem { LocationId = locMain.Id,   Title = "Vươn xa vạn lý",           Url = "/vuon-xa-van-ly",        SortOrder = 4, IsActive = true };

        // Footer
        var mnuFooterHome  = new MenuItem { LocationId = locFooter.Id, Title = "Trang chủ",        Url = "/",                    SortOrder = 0, IsActive = true };
        var mnuFooterVe    = new MenuItem { LocationId = locFooter.Id, Title = "Về Tân Huê Viên",  Url = "/ve-tan-hue-vien",     SortOrder = 1, IsActive = true };
        var mnuFooterSP    = new MenuItem { LocationId = locFooter.Id, Title = "Sản phẩm",         Url = "/san-pham",            SortOrder = 2, IsActive = true };
        var mnuFooterTT    = new MenuItem { LocationId = locFooter.Id, Title = "Truyền thông",     Url = "/truyen-thong",        SortOrder = 3, IsActive = true };
        var mnuFooterVuon  = new MenuItem { LocationId = locFooter.Id, Title = "Vươn xa vạn lý",  Url = "/vuon-xa-van-ly",      SortOrder = 4, IsActive = true };
        var mnuFooterLienHe= new MenuItem { LocationId = locFooter.Id, Title = "Liên hệ",          Url = "/lien-he",             SortOrder = 5, IsActive = true };

        context.MenuItems.AddRange(
            mnuVeTHV, mnuThamQuan, mnuHoTro, mnuTuyenDung,
            mnuSanPham, mnuNangLuc, mnuTruyenThong, mnuTrachNhiem, mnuVuonXa,
            mnuFooterHome, mnuFooterVe, mnuFooterSP, mnuFooterTT, mnuFooterVuon, mnuFooterLienHe
        );
        await context.SaveChangesAsync(CancellationToken.None); // → ID được gán

        // ── PASS 2: Menu con (Children) ──────────────────────────────────────
        context.MenuItems.AddRange(
            // Về Tân Huê Viên
            new MenuItem { LocationId = locTop.Id,  ParentId = mnuVeTHV.Id,    Title = "Câu trích dẫn của TGĐ",  Url = "/ve-tan-hue-vien#ceo-quote",       SortOrder = 0, IsActive = true },
            new MenuItem { LocationId = locTop.Id,  ParentId = mnuVeTHV.Id,    Title = "Tầm nhìn",               Url = "/ve-tan-hue-vien#tam-nhin",         SortOrder = 1, IsActive = true },
            new MenuItem { LocationId = locTop.Id,  ParentId = mnuVeTHV.Id,    Title = "Sứ mệnh",                Url = "/ve-tan-hue-vien#su-menh",          SortOrder = 2, IsActive = true },
            new MenuItem { LocationId = locTop.Id,  ParentId = mnuVeTHV.Id,    Title = "Giá trị cốt lõi",        Url = "/ve-tan-hue-vien#gia-tri-cot-loi",  SortOrder = 3, IsActive = true },
            new MenuItem { LocationId = locTop.Id,  ParentId = mnuVeTHV.Id,    Title = "Hành trình Tân Huê Viên",Url = "/ve-tan-hue-vien#hanh-trinh",       SortOrder = 4, IsActive = true },
            new MenuItem { LocationId = locTop.Id,  ParentId = mnuVeTHV.Id,    Title = "Thành tựu",              Url = "/ve-tan-hue-vien#thanh-tuu",        SortOrder = 5, IsActive = true },
            new MenuItem { LocationId = locTop.Id,  ParentId = mnuVeTHV.Id,    Title = "Happy Land",             Url = "/ve-tan-hue-vien#happy-land",       SortOrder = 6, IsActive = true },
            // Sản phẩm
            new MenuItem { LocationId = locMain.Id, ParentId = mnuSanPham.Id,  Title = "Danh sách sản phẩm",     Url = "/san-pham",                         SortOrder = 0, IsActive = true },
            new MenuItem { LocationId = locMain.Id, ParentId = mnuSanPham.Id,  Title = "Chương trình khuyến mãi",Url = "/khuyen-mai",                        SortOrder = 1, IsActive = true },
            // Năng lực sản xuất
            new MenuItem { LocationId = locMain.Id, ParentId = mnuNangLuc.Id,  Title = "Nhà xưởng",              Url = "/nha-xuong",                        SortOrder = 0, IsActive = true },
            new MenuItem { LocationId = locMain.Id, ParentId = mnuNangLuc.Id,  Title = "Chứng nhận",             Url = "/chung-nhan",                       SortOrder = 1, IsActive = true },
            // Vươn xa vạn lý
            new MenuItem { LocationId = locMain.Id, ParentId = mnuVuonXa.Id,   Title = "Hệ thống nội địa",       Url = "/vuon-xa-van-ly/noi-dia",           SortOrder = 0, IsActive = true },
            new MenuItem { LocationId = locMain.Id, ParentId = mnuVuonXa.Id,   Title = "Thị trường xuất khẩu",   Url = "/vuon-xa-van-ly/xuat-khau",         SortOrder = 1, IsActive = true }
        );

        await context.SaveChangesAsync(CancellationToken.None);
    }
}
