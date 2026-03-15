using Microsoft.EntityFrameworkCore;
using SPage.Domain.Entities.Page;
using SPage.Domain.Enums;
using SPage.Infrastructure.Persistence;

namespace SPage.Web.SeedData;

internal static partial class DataSeeder
{
    /// <summary>
    /// Seed DynamicPage "Tham quan Nhà máy và Liên Hoa".
    /// Toàn bộ nội dung dùng section RawHtml, CssBlock, JsBlock.
    /// </summary>
    public static async Task SeedPagesAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<ApplicationDbContext>();
        const string slug = "tham-quan-nha-may";

        var existing = await context.DynamicPages.IgnoreQueryFilters().Where(p => p.Slug == slug).ToListAsync();
        if (existing.Count > 0)
        {
            context.DynamicPages.RemoveRange(existing);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var page = new DynamicPage
        {
            Title = "Tham quan Nhà máy và Liên Hoa",
            Slug = slug,
            MetaTitle = "Tham quan Nhà máy và Liên Hoa – Đăng ký miễn phí",
            MetaDescription = "Đăng ký tham quan nhà xưởng sản xuất và trung tâm Liên Hoa Bảo Tháp. Trải nghiệm thực tế quy trình sản xuất bánh, xe điện tham quan, giao lưu giải đáp.",
            IsIndexable = true,
            Status = PageStatus.Published,
            Template = "Default",
            Sections =
            [
                new PageSectionData { SectionType = "RawHtml", SortOrder = 0, IsVisible = true, Data = new Dictionary<string, string> { ["content"] = GetThamQuanHeroHtml() } },
                new PageSectionData { SectionType = "RawHtml", SortOrder = 1, IsVisible = true, Data = new Dictionary<string, string> { ["content"] = GetThamQuanTableHtml() } },
                new PageSectionData { SectionType = "RawHtml", SortOrder = 2, IsVisible = true, Data = new Dictionary<string, string> { ["content"] = GetThamQuanDiaDiemHtml() } },
                new PageSectionData { SectionType = "RawHtml", SortOrder = 3, IsVisible = true, Data = new Dictionary<string, string> { ["content"] = GetThamQuanPhuongTienHtml() } },
                new PageSectionData { SectionType = "RawHtml", SortOrder = 4, IsVisible = true, Data = new Dictionary<string, string> { ["content"] = GetThamQuanQuyDinhHtml() } },
                new PageSectionData { SectionType = "RawHtml", SortOrder = 5, IsVisible = true, Data = new Dictionary<string, string> { ["content"] = GetThamQuanCachThucHtml() } },
                new PageSectionData { SectionType = "RawHtml", SortOrder = 6, IsVisible = true, Data = new Dictionary<string, string> { ["content"] = GetThamQuanGalleryHtml() } },
                new PageSectionData { SectionType = "RawHtml", SortOrder = 7, IsVisible = true, Data = new Dictionary<string, string> { ["content"] = GetThamQuanFormHtml() } },
                new PageSectionData { SectionType = "JsBlock", SortOrder = 8, IsVisible = true, Data = new Dictionary<string, string> { ["content"] = GetThamQuanFormScript() } },
            ]
        };

        context.DynamicPages.Add(page);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    private static string GetThamQuanHeroHtml() =>
        """
        <section class="ac-hero" aria-label="Banner tham quan">
            <div class="hd-container ac-hero__inner">
                <div class="ac-hero__left">
                    <div class="ac-hero__badge">
                        <img src="/logo/logo-40-nam.png" alt="Tân Huê Viên 40 năm" class="ac-hero__badge-logo" />
                    </div>
                    <h1 class="ac-hero__tagline">Tham quan <em>Nhà máy</em> &amp; Liên Hoa</h1>
                    <div class="ac-hero__sub-wrap" style="display:block;min-height:30px;margin-bottom:var(--spacing-8)">
                        <p class="ac-hero__sub" style="margin:0">Trải nghiệm thực tế quy trình sản xuất – Khám phá Liên Hoa Bảo Tháp – Giao lưu cùng đội ngũ chuyên gia</p>
                    </div>
                    <a href="#form-dang-ky" class="ac-btn ac-btn--white">Đăng ký tham quan <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg></a>
                </div>
                <div class="ac-hero__right" aria-hidden="true">
                    <div class="ac-hero__deco-circle ac-hero__deco-circle--1"></div>
                    <div class="ac-hero__deco-circle ac-hero__deco-circle--2"></div>
                </div>
            </div>
            <div class="ac-wave" aria-hidden="true">
                <svg viewBox="0 0 1440 100" preserveAspectRatio="none" xmlns="http://www.w3.org/2000/svg"><path fill="#ffffff" d="M0,60 C240,110 480,10 720,60 C960,110 1200,20 1440,60 L1440,100 L0,100 Z"/></svg>
            </div>
        </section>
        """;

    private static string GetThamQuanTableHtml() =>
        """
        <section class="ac-section" aria-labelledby="tour-content-title">
            <div class="hd-container">
                <h2 class="ac-section-title ac-section-title--red" id="tour-content-title" data-reveal>NỘI DUNG CHUYẾN THAM QUAN</h2>
                <div class="ac-tour-table-wrap" data-reveal>
                    <table class="ac-tour-table">
                        <thead><tr><th>Hạng mục</th><th>Chi tiết</th></tr></thead>
                        <tbody>
                            <tr><td><strong>Địa điểm</strong></td><td>Nhà xưởng sản xuất &amp; Liên Hoa Bảo Tháp (LHBT)</td></tr>
                            <tr><td><strong>Phương tiện</strong></td><td>Xe điện tham quan &amp; tham quan tượng Phật Dược Sư</td></tr>
                            <tr><td><strong>Quy định</strong></td><td>Trang phục lịch sự, không mang thức ăn vào nhà xưởng, giữ gìn vệ sinh chung</td></tr>
                            <tr><td><strong>Cách thức đăng ký</strong></td><td>Điền biểu mẫu bên dưới, thưởng thức bánh nóng &amp; giao lưu giải đáp</td></tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </section>
        <style>.ac-tour-table-wrap{overflow-x:auto;margin-top:1rem;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,0.06);border:1px solid rgba(201,162,62,0.2);}.ac-tour-table{width:100%;border-collapse:collapse;font-size:0.9375rem;}.ac-tour-table th,.ac-tour-table td{padding:0.875rem 1rem;text-align:left;border-bottom:1px solid #e5e7eb;}.ac-tour-table th{width:22%;font-weight:600;color:#374151;background:#faf8f2;}.ac-tour-table td{color:#4b5563;}.ac-tour-table tbody tr:last-child td{border-bottom:0;}</style>
        """;

    private static string GetThamQuanDiaDiemHtml() =>
        """
        <section class="ac-section ac-about" aria-labelledby="tour-place-title">
            <div class="ac-wave ac-wave--top" aria-hidden="true"><svg viewBox="0 0 1440 80" preserveAspectRatio="none" xmlns="http://www.w3.org/2000/svg"><path fill="#eec976" d="M0,40 C360,-10 1080,80 1440,30 L1440,0 L0,0 Z"/></svg></div>
            <div class="hd-container ac-about__inner">
                <div class="ac-about__left" data-reveal-x style="--reveal-x:-40px">
                    <h2 class="ac-about__title" id="tour-place-title">ĐỊA ĐIỂM THAM QUAN</h2>
                    <p class="ac-about__desc">Chuyến tham quan diễn ra tại các khu vực chính:</p>
                    <ul class="ac-tour-list">
                        <li><strong>Nhà xưởng sản xuất</strong> – Tận mắt chứng kiến dây chuyền sản xuất bánh hiện đại, từ khâu nguyên liệu đến thành phẩm đóng gói theo tiêu chuẩn an toàn vệ sinh thực phẩm.</li>
                        <li><strong>Liên Hoa Bảo Tháp (LHBT)</strong> – Không gian tâm linh và văn hóa độc đáo, nơi lưu giữ những giá trị truyền thống hơn 40 năm của thương hiệu.</li>
                        <li><strong>Phòng truyền thống 40 năm</strong> – Triển lãm lịch sử hình thành và phát triển của nhà máy qua các thời kỳ.</li>
                    </ul>
                </div>
            </div>
            <div class="ac-wave ac-wave--bottom" aria-hidden="true"><svg viewBox="0 0 1440 100" preserveAspectRatio="none" xmlns="http://www.w3.org/2000/svg"><path fill="#ffffff" d="M0,60 C240,110 480,10 720,60 C960,110 1200,20 1440,60 L1440,100 L0,100 Z"/></svg></div>
        </section>
        <style>.ac-tour-list{margin:0.75rem 0 0 1.25rem;padding:0;list-style:disc;color:rgba(255,255,255,0.95);}.ac-tour-list li{margin-bottom:0.5rem;line-height:1.6;}</style>
        """;

    private static string GetThamQuanPhuongTienHtml() =>
        """
        <section class="ac-section" aria-labelledby="tour-transport-title">
            <div class="hd-container">
                <h2 class="ac-section-title ac-section-title--red" id="tour-transport-title" data-reveal>PHƯƠNG TIỆN DI CHUYỂN</h2>
                <div class="ac-tour-prose" data-reveal>
                    <p>Trong suốt hành trình tham quan, đoàn khách sẽ được di chuyển bằng:</p>
                    <ul>
                        <li><strong>Xe điện tham quan</strong> – Phương tiện thân thiện với môi trường, đưa đoàn dạo quanh toàn bộ khuôn viên nhà máy một cách thoải mái.</li>
                        <li><strong>Tham quan tượng Phật Dược Sư</strong> – Điểm dừng chân đặc biệt trong khuôn viên Liên Hoa Bảo Tháp, mang lại trải nghiệm văn hóa tâm linh sâu sắc.</li>
                    </ul>
                </div>
            </div>
        </section>
        <style>.ac-tour-prose{max-width:42rem;margin-top:1rem;}.ac-tour-prose ul{margin:0.5rem 0 0 1.25rem;padding:0;}.ac-tour-prose li{margin-bottom:0.5rem;line-height:1.6;color:#4b5563;}</style>
        """;

    private static string GetThamQuanQuyDinhHtml() =>
        """
        <section class="ac-section ac-ceo" aria-labelledby="tour-rules-title">
            <div class="hd-container ac-ceo__inner">
                <div class="ac-ceo__text" data-reveal>
                    <h2 class="ac-section-title ac-section-title--red" id="tour-rules-title" style="margin-bottom:1rem">QUY ĐỊNH KHI THAM QUAN</h2>
                    <p class="ac-ceo__body">Để đảm bảo an toàn và chất lượng tham quan, quý khách vui lòng tuân thủ:</p>
                    <p class="ac-ceo__body"><strong>Khu vực nhà xưởng &amp; LHBT:</strong> Mặc trang phục gọn gàng, lịch sự; không mang thức ăn vào khu vực sản xuất; không chụp ảnh nơi có biển cấm; giữ gìn vệ sinh; trẻ em dưới 10 tuổi cần người lớn đi kèm.</p>
                    <p class="ac-ceo__body"><strong>Phòng truyền thống 40 năm:</strong> Không chạm vào hiện vật; giữ yên lặng.</p>
                </div>
            </div>
        </section>
        """;

    private static string GetThamQuanCachThucHtml() =>
        """
        <section class="ac-section" aria-labelledby="tour-how-title">
            <div class="hd-container">
                <h2 class="ac-section-title ac-section-title--red" id="tour-how-title" data-reveal>CÁCH THỨC ĐĂNG KÝ</h2>
                <div class="ac-tour-prose" data-reveal>
                    <p>Đăng ký tham quan hoàn toàn <strong>miễn phí</strong>. Quy trình gồm 3 bước:</p>
                    <ol style="margin:0.5rem 0 0 1.25rem;padding:0;color:#4b5563;line-height:1.8">
                        <li><strong>Điền biểu mẫu đăng ký</strong> – Điền đầy đủ thông tin vào mẫu bên dưới (họ tên, email, số người, ngày dự kiến, ghi chú).</li>
                        <li><strong>Thưởng thức bánh nóng</strong> – Sau tham quan, đoàn được mời thưởng thức bánh nóng trực tiếp ra lò.</li>
                        <li><strong>Giao lưu &amp; giải đáp</strong> – Đội ngũ chuyên gia giải đáp mọi thắc mắc về quy trình sản xuất và sản phẩm.</li>
                    </ol>
                    <p style="margin-top:1.5rem"><a href="#form-dang-ky" class="ac-btn ac-btn--red">Điền phiếu đăng ký ngay <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg></a></p>
                </div>
            </div>
        </section>
        """;

    private static string GetThamQuanGalleryHtml() =>
        """
        <section class="ac-section" aria-labelledby="tour-gallery-title">
            <div class="hd-container">
                <h2 class="ac-section-title ac-section-title--red" id="tour-gallery-title" data-reveal>HÌNH ẢNH CÁC ĐOÀN THAM QUAN</h2>
                <div class="ac-tour-gallery" data-reveal>
                    <div class="ac-tour-gallery__grid">
                        <img src="/images/tour/tour-01.jpg" alt="Đoàn khách tham quan nhà xưởng" loading="lazy" decoding="async" />
                        <img src="/images/tour/tour-02.jpg" alt="Xe điện đưa đoàn tham quan khuôn viên" loading="lazy" decoding="async" />
                        <img src="/images/tour/tour-03.jpg" alt="Tham quan Liên Hoa Bảo Tháp" loading="lazy" decoding="async" />
                        <img src="/images/tour/tour-04.jpg" alt="Thưởng thức bánh nóng sau tham quan" loading="lazy" decoding="async" />
                        <img src="/images/tour/tour-05.jpg" alt="Giao lưu giải đáp cùng chuyên gia" loading="lazy" decoding="async" />
                        <img src="/images/tour/tour-06.jpg" alt="Phòng truyền thống 40 năm" loading="lazy" decoding="async" />
                    </div>
                </div>
            </div>
        </section>
        <style>.ac-tour-gallery__grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(160px,1fr));gap:1rem;margin-top:1rem;}.ac-tour-gallery__grid img{width:100%;aspect-ratio:1;object-fit:cover;border-radius:12px;box-shadow:0 2px 12px rgba(0,0,0,0.08);}</style>
        """;

    private static string GetThamQuanFormHtml() =>
        """
        <section class="ac-section ac-contact" id="form-dang-ky" aria-labelledby="tour-form-title">
            <div class="hd-container ac-contact__container" style="max-width:42rem">
                <header class="ac-contact__header" data-reveal>
                    <p class="ac-contact__eyebrow">Đăng ký miễn phí</p>
                    <h2 class="ac-contact__title" id="tour-form-title">Đăng ký tham quan Nhà máy và Liên Hoa</h2>
                    <p class="ac-contact__intro">Quý khách vui lòng điền form bên dưới. Chúng tôi sẽ liên hệ xác nhận trong thời gian sớm nhất.</p>
                </header>
                <div class="ac-contact__card-wrap" data-reveal>
                    <form id="form-dang-ky" class="ac-contact__form" novalidate>
                        <input type="hidden" name="subject" value="Đăng ký tham quan Nhà máy và Liên Hoa" />
                        <div class="ac-contact__grid">
                            <div class="ac-contact__field">
                                <label for="tq-hoten" class="ac-contact__label">Họ và tên <span class="ac-contact__required">*</span></label>
                                <input type="text" id="tq-hoten" name="hoTen" class="ac-contact__input" placeholder="Nguyễn Văn A" required autocomplete="name" />
                            </div>
                            <div class="ac-contact__field">
                                <label for="tq-email" class="ac-contact__label">Email</label>
                                <input type="email" id="tq-email" name="email" class="ac-contact__input" placeholder="email@example.com" autocomplete="email" />
                            </div>
                            <div class="ac-contact__field">
                                <label for="tq-phone" class="ac-contact__label">Số điện thoại <span class="ac-contact__required">*</span></label>
                                <input type="tel" id="tq-phone" name="phone" class="ac-contact__input" placeholder="0901 234 567" required autocomplete="tel" />
                            </div>
                            <div class="ac-contact__field">
                                <label for="tq-soluong" class="ac-contact__label">Số lượng người tham quan</label>
                                <input type="number" id="tq-soluong" name="soLuong" class="ac-contact__input" placeholder="Ví dụ: 10" min="1" />
                            </div>
                            <div class="ac-contact__field" style="grid-column:1/-1">
                                <label for="tq-ngay" class="ac-contact__label">Ngày dự kiến tham quan</label>
                                <input type="text" id="tq-ngay" name="ngayDuKien" class="ac-contact__input" placeholder="Ví dụ: 20/06/2025" />
                            </div>
                        </div>
                        <div class="ac-contact__field">
                            <label for="tq-ghichu" class="ac-contact__label">Ghi chú / Yêu cầu đặc biệt</label>
                            <textarea id="tq-ghichu" name="ghiChu" class="ac-contact__input ac-contact__textarea" rows="4" placeholder="Thời gian mong muốn, yêu cầu đặc biệt..."></textarea>
                        </div>
                        <div class="ac-contact__footer">
                            <button type="submit" class="ac-contact__submit" id="form-dang-ky-submit">
                                <span class="ac-contact__submit-text">Gửi đăng ký</span>
                                <svg class="ac-contact__submit-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 2L11 13M22 2l-7 20-4-9-9-4 20-7z"/></svg>
                            </button>
                            <div id="form-dang-ky-result" class="ac-contact__result" aria-live="polite"></div>
                        </div>
                    </form>
                </div>
            </div>
        </section>
        <style>
        #form-dang-ky.ac-section{background:linear-gradient(180deg,#fefdf9 0%,#faf8f2 50%,#fff 100%);padding:var(--spacing-12,3rem) 0 var(--spacing-16,4rem);}
        .ac-contact__container{max-width:42rem;margin:0 auto;}
        .ac-contact__header{text-align:center;margin-bottom:2.5rem;}
        .ac-contact__eyebrow{font-size:0.75rem;font-weight:600;letter-spacing:0.2em;text-transform:uppercase;color:var(--color-primary-dark,#c9a23e);margin:0 0 0.5rem;}
        .ac-contact__title{font-size:clamp(1.75rem,4vw,2.25rem);font-weight:700;letter-spacing:-0.02em;color:#1a1a1a;margin:0 0 0.75rem;line-height:1.2;}
        .ac-contact__intro{font-size:0.9375rem;color:var(--color-text-muted,#6b7280);line-height:1.6;margin:0;max-width:28rem;margin-left:auto;margin-right:auto;}
        .ac-contact__card-wrap{background:#fff;border-radius:12px;box-shadow:0 4px 24px rgba(0,0,0,0.06),0 0 0 1px rgba(201,162,62,0.12);padding:2rem;}
        .ac-contact__form{display:flex;flex-direction:column;gap:1.5rem;}
        .ac-contact__grid{display:grid;grid-template-columns:1fr 1fr;gap:1.25rem 1.5rem;}
        @media (max-width:520px){.ac-contact__grid{grid-template-columns:1fr;}}
        .ac-contact__field{display:flex;flex-direction:column;gap:0.375rem;}
        .ac-contact__label{font-size:0.8125rem;font-weight:600;color:#374151;}
        .ac-contact__required{color:#b91c1c;}
        .ac-contact__input{width:100%;padding:0.75rem 1rem;font-size:0.9375rem;border:1px solid #e5e7eb;border-radius:8px;background:#fafafa;}
        .ac-contact__input:focus{outline:none;border-color:#c9a23e;box-shadow:0 0 0 3px rgba(201,162,62,0.15);}
        .ac-contact__textarea{resize:vertical;min-height:100px;}
        .ac-contact__footer{display:flex;align-items:center;gap:1rem;flex-wrap:wrap;}
        .ac-contact__submit{display:inline-flex;align-items:center;gap:0.5rem;padding:0.75rem 1.5rem;font-size:0.9375rem;font-weight:600;color:#fff;background:linear-gradient(135deg,#c9a23e 0%,#a8841e 100%);border:none;border-radius:8px;cursor:pointer;box-shadow:0 2px 8px rgba(201,162,62,0.35);}
        .ac-contact__submit:hover:not(:disabled){transform:translateY(-1px);}
        .ac-contact__submit:disabled{opacity:0.7;cursor:not-allowed;}
        .ac-contact__result{font-size:0.875rem;}
        .ac-contact__result .text-success{color:#15803d;}
        .ac-contact__result .text-danger{color:#b91c1c;}
        </style>
        """;

    private static string GetThamQuanFormScript() =>
        """
        (function() {
          $('#form-dang-ky').on('submit', function(e) {
            e.preventDefault();
            var $form = $(this), $result = $('#form-dang-ky-result'), $btn = $('#form-dang-ky-submit');
            var fields = {};
            $form.find('[name]').each(function() { var n = $(this).attr('name'); if (n && n !== 'subject') fields[n] = ($(this).val() || '').trim(); });
            var subject = $form.find('input[name="subject"]').val() || 'Đăng ký tham quan Nhà máy và Liên Hoa';
            if (!fields.hoTen || !fields.phone) { $result.html('<span class="text-danger">Vui lòng điền Họ tên và Số điện thoại.</span>'); return false; }
            $result.empty();
            $btn.prop('disabled', true);
            $.ajax({ url: '/api/contact', method: 'POST', contentType: 'application/json', data: JSON.stringify({ subject: subject, fields: fields }) })
              .done(function(res) { $result.html('<span class="text-success">✓ ' + (res.message || 'Đã gửi! Chúng tôi sẽ liên hệ xác nhận sớm nhất.') + '</span>'); $form[0].reset(); })
              .fail(function(xhr) { var msg = (xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : 'Gửi thất bại. Vui lòng thử lại.'; $result.html('<span class="text-danger">' + $('<div>').text(msg).html() + '</span>'); })
              .always(function() { $btn.prop('disabled', false); });
            return false;
          });
        })();
        """;
}
