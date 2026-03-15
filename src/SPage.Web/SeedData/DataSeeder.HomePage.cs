using Microsoft.EntityFrameworkCore;
using SPage.Domain.Entities.Page;
using SPage.Domain.Enums;
using SPage.Infrastructure.Persistence;

namespace SPage.Web.SeedData;

/// <summary>
/// Seed trang động "Trang chủ" — thiết kế giống Home/Index, nội dung RawHtml + JS lấy sản phẩm/tin tức từ API.
/// </summary>
internal static partial class DataSeeder
{
    public static async Task SeedHomePageAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<ApplicationDbContext>();
        const string slug = "trang-chu";

        var exists = await context.DynamicPages.IgnoreQueryFilters().AnyAsync(p => p.Slug == slug);
        if (exists) return;

        var page = new DynamicPage
        {
            Title = "Trang chủ",
            Slug = slug,
            MetaTitle = "Trang chủ — Đặc sản Sóc Trăng",
            MetaDescription = "Tân Huê Viên — Bánh Pía, Lạp Xưởng, Kẹo. Thương hiệu tin cậy từ năm 1985.",
            IsIndexable = true,
            Status = PageStatus.Published,
            Template = "Default",
            Sections =
            [
                new PageSectionData
                {
                    SectionType = "RawHtml",
                    SortOrder = 0,
                    IsVisible = true,
                    Data = new Dictionary<string, string> { ["content"] = GetHeroHtml() }
                },
                new PageSectionData
                {
                    SectionType = "RawHtml",
                    SortOrder = 1,
                    IsVisible = true,
                    Data = new Dictionary<string, string> { ["content"] = GetPromosWrapperHtml() }
                },
                new PageSectionData
                {
                    SectionType = "RawHtml",
                    SortOrder = 2,
                    IsVisible = true,
                    Data = new Dictionary<string, string> { ["content"] = GetAboutAndMoreHtml() }
                },
                new PageSectionData
                {
                    SectionType = "RawHtml",
                    SortOrder = 3,
                    IsVisible = true,
                    Data = new Dictionary<string, string> { ["content"] = GetNewsWrapperHtml() }
                },
                new PageSectionData
                {
                    SectionType = "RawHtml",
                    SortOrder = 4,
                    IsVisible = true,
                    Data = new Dictionary<string, string> { ["content"] = GetContactFormHtml() }
                },
                new PageSectionData
                {
                    SectionType = "JsBlock",
                    SortOrder = 5,
                    IsVisible = true,
                    Data = new Dictionary<string, string> { ["content"] = GetHomePageScript() }
                }
            ]
        };

        context.DynamicPages.Add(page);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    private static string GetHeroHtml() =>
        """
        <section class="ac-hero" aria-label="Banner trang chủ">
            <div class="hd-container ac-hero__inner">
                <div class="ac-hero__left">
                    <div class="ac-hero__badge">
                        <img src="/logo/logo-40-nam.png" alt="Tân Huê Viên 40 năm" class="ac-hero__badge-logo" />
                    </div>
                    <h1 class="ac-hero__tagline">Đặc Sản <em>Sóc Trăng</em></h1>
                    <div class="ac-hero__sub-wrap" style="display:block;min-height:30px;margin-bottom:var(--spacing-8)">
                        <p class="ac-hero__sub" style="margin:0">Bánh Pía · Lạp Xưởng · Kẹo — Thương hiệu tin cậy từ năm 1985</p>
                    </div>
                    <a href="/san-pham" class="ac-btn ac-btn--white">Khám phá sản phẩm <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg></a>
                </div>
                <div class="ac-hero__right" aria-hidden="true">
                    <div class="ac-hero__product ac-hero__product--main ac-hero__product--sway">
                        <img src="/images/home/hero-banh-pia-kim-sa.png" alt="Bánh Pía Kim Sa" class="ac-hero__product-img ac-hero__product-img--main" />
                    </div>
                    <div class="ac-hero__product ac-hero__product--sub ac-hero__product--sway-alt">
                        <img src="/images/home/hero-lap-xuong.png" alt="Lạp Xưởng Tươi" class="ac-hero__product-img ac-hero__product-img--sub" />
                    </div>
                    <div class="ac-hero__deco-circle ac-hero__deco-circle--1"></div>
                    <div class="ac-hero__deco-circle ac-hero__deco-circle--2"></div>
                </div>
            </div>
            <div class="ac-wave" aria-hidden="true">
                <svg viewBox="0 0 1440 100" preserveAspectRatio="none" xmlns="http://www.w3.org/2000/svg"><path fill="#ffffff" d="M0,60 C240,110 480,10 720,60 C960,110 1200,20 1440,60 L1440,100 L0,100 Z"/></svg>
            </div>
        </section>
        """;

    private static string GetPromosWrapperHtml() =>
        """
        <section class="ac-section ac-promos" aria-label="Sản phẩm nổi bật">
            <div class="hd-container">
                <div id="home-featured-products" class="ac-promos__grid">
                    <div class="ac-promos__loading" style="padding:2rem;text-align:center;color:#94a3b8">Đang tải...</div>
                </div>
            </div>
        </section>
        """;

    private static string GetAboutAndMoreHtml() =>
        """
        <section class="ac-about" aria-labelledby="about-title">
            <div class="ac-wave ac-wave--top" aria-hidden="true">
                <svg viewBox="0 0 1440 80" preserveAspectRatio="none" xmlns="http://www.w3.org/2000/svg"><path fill="#eec976" d="M0,40 C360,-10 1080,80 1440,30 L1440,0 L0,0 Z"/></svg>
            </div>
            <div class="hd-container ac-about__inner">
                <div class="ac-about__left" data-reveal-x style="--reveal-x:-40px">
                    <h2 class="ac-about__title" id="about-title">THƯƠNG HIỆU<br/>ĐẶC SẢN<br/>SÓC TRĂNG</h2>
                    <p class="ac-about__desc">Công ty Tân Huê Viên được thành lập từ năm 1985, chuyên sản xuất và kinh doanh các loại bánh kẹo Đặc sản Sóc Trăng. Là một trong những doanh nghiệp đi đầu trong việc bảo tồn truyền thống và không ngừng cải tiến máy móc công nghệ hiện đại.</p>
                    <a href="/about" class="ac-btn ac-btn--white-outline">Xem thêm <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg></a>
                </div>
                <div class="ac-about__right">
                    <div class="ac-stat-big" data-reveal><strong>40+</strong><span>NĂM KINH NGHIỆM</span></div>
                    <div class="ac-stat-big" data-reveal data-reveal-delay="100"><strong>20+</strong><span>QUỐC GIA XUẤT KHẨU</span></div>
                </div>
            </div>
            <div class="ac-wave ac-wave--bottom" aria-hidden="true">
                <svg viewBox="0 0 1440 100" preserveAspectRatio="none" xmlns="http://www.w3.org/2000/svg"><path fill="#ffffff" d="M0,60 C240,110 480,10 720,60 C960,110 1200,20 1440,60 L1440,100 L0,100 Z"/></svg>
            </div>
        </section>
        <section class="ac-section ac-ceo">
            <div class="hd-container ac-ceo__inner">
                <div class="ac-ceo__text" data-reveal>
                    <p class="ac-ceo__body">Tân Huê Viên tự hào là doanh nghiệp tiên phong trong việc bảo tồn và phát triển hương vị bánh pía truyền thống Sóc Trăng. Qua hơn 40 năm hình thành, chúng tôi đã không ngừng đổi mới.</p>
                    <p class="ac-ceo__sign"><strong>Công ty TNHH Tân Huê Viên — Từ năm 1985</strong></p>
                </div>
            </div>
        </section>
        <section class="ac-section ac-quality" aria-labelledby="quality-title">
            <div class="hd-container ac-quality__inner">
                <div class="ac-quality__text" data-reveal-x style="--reveal-x:-40px">
                    <div class="ac-eyebrow">Cam kết chất lượng</div>
                    <h2 class="ac-quality__title" id="quality-title">QUY TRÌNH SẢN XUẤT 5 BƯỚC</h2>
                    <a href="/chat-luong" class="ac-btn ac-btn--red">Xem chi tiết <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg></a>
                </div>
            </div>
        </section>
        <section class="ac-sustain" aria-labelledby="sustain-title">
            <div class="hd-container ac-sustain__inner">
                <div class="ac-sustain__content" data-reveal-x style="--reveal-x:40px">
                    <div class="ac-eyebrow ac-eyebrow--green">Phân phối rộng khắp</div>
                    <h2 class="ac-sustain__title" id="sustain-title">MẠNG LƯỚI PHÂN PHỐI TOÀN CẦU</h2>
                    <a href="/dai-ly" class="ac-btn ac-btn--green">Tìm đại lý <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M5 12h14M12 5l7 7-7 7"/></svg></a>
                </div>
            </div>
        </section>
        """;

    private static string GetNewsWrapperHtml() =>
        """
        <section class="ac-section ac-news" aria-labelledby="news-title">
            <div class="hd-container">
                <h2 class="ac-section-title ac-section-title--red" id="news-title" data-reveal>TIN TỨC SỰ KIỆN</h2>
                <div id="home-recent-posts" class="ac-news-grid">
                    <div class="ac-news-loading" style="padding:2rem;text-align:center;color:#94a3b8">Đang tải...</div>
                </div>
            </div>
        </section>
        """;

    private static string GetContactFormHtml() =>
        """
        <section class="ac-section ac-contact" id="home-contact" aria-labelledby="contact-title">
            <div class="hd-container ac-contact__container">
                <header class="ac-contact__header" data-reveal>
                    <p class="ac-contact__eyebrow">Liên hệ với chúng tôi</p>
                    <h2 class="ac-contact__title" id="contact-title">Gửi tin nhắn</h2>
                    <p class="ac-contact__intro">Quý khách vui lòng điền form bên dưới. Chúng tôi sẽ phản hồi trong thời gian sớm nhất.</p>
                </header>
                <div class="ac-contact__card-wrap" data-reveal>
                    <form id="home-contact-form" class="ac-contact__form" novalidate>
                        <div class="ac-contact__grid">
                            <div class="ac-contact__field">
                                <label for="home-cf-hoten" class="ac-contact__label">Họ và tên <span class="ac-contact__required" aria-hidden="true">*</span></label>
                                <input type="text" id="home-cf-hoten" name="hoTen" class="ac-contact__input" placeholder="Nguyễn Văn A" required autocomplete="name" />
                            </div>
                            <div class="ac-contact__field">
                                <label for="home-cf-email" class="ac-contact__label">Email</label>
                                <input type="email" id="home-cf-email" name="email" class="ac-contact__input" placeholder="email@example.com" autocomplete="email" />
                            </div>
                            <div class="ac-contact__field">
                                <label for="home-cf-phone" class="ac-contact__label">Số điện thoại <span class="ac-contact__required" aria-hidden="true">*</span></label>
                                <input type="tel" id="home-cf-phone" name="phone" class="ac-contact__input" placeholder="0901 234 567" required autocomplete="tel" />
                            </div>
                        </div>
                        <div class="ac-contact__field">
                            <label for="home-cf-ghichu" class="ac-contact__label">Nội dung</label>
                            <textarea id="home-cf-ghichu" name="ghiChu" class="ac-contact__input ac-contact__textarea" rows="4" placeholder="Nội dung cần trao đổi hoặc câu hỏi của quý khách..."></textarea>
                        </div>
                        <div class="ac-contact__footer">
                            <button type="submit" class="ac-contact__submit" id="home-contact-submit">
                                <span class="ac-contact__submit-text">Gửi tin nhắn</span>
                                <svg class="ac-contact__submit-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 2L11 13M22 2l-7 20-4-9-9-4 20-7z"/></svg>
                            </button>
                            <div id="home-contact-result" class="ac-contact__result" aria-live="polite"></div>
                        </div>
                    </form>
                </div>
            </div>
        </section>
        <style>
        #home-contact { background: linear-gradient(180deg, #fefdf9 0%, #faf8f2 50%, #fff 100%); padding: var(--spacing-12, 3rem) 0 var(--spacing-16, 4rem); }
        .ac-contact__container { max-width: 42rem; margin: 0 auto; }
        .ac-contact__header { text-align: center; margin-bottom: 2.5rem; }
        .ac-contact__eyebrow { font-size: 0.75rem; font-weight: 600; letter-spacing: 0.2em; text-transform: uppercase; color: var(--color-primary-dark, #c9a23e); margin: 0 0 0.5rem; }
        .ac-contact__title { font-size: clamp(1.75rem, 4vw, 2.25rem); font-weight: 700; letter-spacing: -0.02em; color: #1a1a1a; margin: 0 0 0.75rem; line-height: 1.2; }
        .ac-contact__intro { font-size: 0.9375rem; color: var(--color-text-muted, #6b7280); line-height: 1.6; margin: 0; max-width: 28rem; margin-left: auto; margin-right: auto; }
        .ac-contact__card-wrap { background: #fff; border-radius: 12px; box-shadow: 0 4px 24px rgba(0,0,0,0.06), 0 0 0 1px rgba(201,162,62,0.12); padding: 2rem; transition: box-shadow 0.3s ease; }
        .ac-contact__card-wrap:hover { box-shadow: 0 8px 32px rgba(0,0,0,0.08), 0 0 0 1px rgba(201,162,62,0.18); }
        .ac-contact__form { display: flex; flex-direction: column; gap: 1.5rem; }
        .ac-contact__grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem 1.5rem; }
        @media (max-width: 520px) { .ac-contact__grid { grid-template-columns: 1fr; } }
        .ac-contact__field { display: flex; flex-direction: column; gap: 0.375rem; }
        .ac-contact__label { font-size: 0.8125rem; font-weight: 600; color: #374151; letter-spacing: 0.01em; }
        .ac-contact__required { color: #b91c1c; font-weight: 500; }
        .ac-contact__input { width: 100%; padding: 0.75rem 1rem; font-size: 0.9375rem; line-height: 1.5; color: #1a1a1a; background: #fafafa; border: 1px solid #e5e7eb; border-radius: 8px; transition: border-color 0.2s ease, box-shadow 0.2s ease; }
        .ac-contact__input::placeholder { color: #9ca3af; }
        .ac-contact__input:hover { border-color: #d1d5db; background: #fff; }
        .ac-contact__input:focus { outline: none; border-color: var(--color-primary-dark, #c9a23e); background: #fff; box-shadow: 0 0 0 3px rgba(201,162,62,0.15); }
        .ac-contact__textarea { resize: vertical; min-height: 120px; padding-top: 0.75rem; }
        .ac-contact__footer { display: flex; align-items: center; gap: 1rem; flex-wrap: wrap; padding-top: 0.25rem; }
        .ac-contact__submit { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.75rem 1.5rem; font-size: 0.9375rem; font-weight: 600; color: #fff; background: linear-gradient(135deg, #c9a23e 0%, #a8841e 100%); border: none; border-radius: 8px; cursor: pointer; box-shadow: 0 2px 8px rgba(201,162,62,0.35); transition: transform 0.2s ease, box-shadow 0.2s ease; }
        .ac-contact__submit:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 4px 12px rgba(201,162,62,0.4); }
        .ac-contact__submit:active:not(:disabled) { transform: translateY(0); }
        .ac-contact__submit:disabled { opacity: 0.7; cursor: not-allowed; }
        .ac-contact__submit-icon { flex-shrink: 0; }
        .ac-contact__result { font-size: 0.875rem; line-height: 1.4; min-height: 1.5em; }
        .ac-contact__result .text-success { color: #15803d; font-weight: 500; }
        .ac-contact__result .text-danger { color: #b91c1c; font-weight: 500; }
        </style>
        """;

    private static string GetHomePageScript() =>
        """
        (function() {
          var base = location.origin || '', esc = function(s) { return $('<div>').text(s == null ? '' : s).html(); };
          var fallbackProduct = '<a href="' + base + '/san-pham" class="ac-promo-card ac-promo-card--lg" data-reveal><div class="ac-promo-card__content"><img src="' + base + '/logo/logo.png" alt="Tân Huê Viên" class="ac-promo-card__brand-logo" /><p class="ac-promo-card__sub">Đặc sản Sóc Trăng</p><h2 class="ac-promo-card__big">Sản phẩm</h2><p class="ac-promo-card__desc">Khám phá các sản phẩm đặc sản.</p></div><div class="ac-promo-card__img-lg" style="background:linear-gradient(135deg,#eec976,#9a7e2e);min-height:200px;display:flex;align-items:center;justify-content:center;color:rgba(255,255,255,0.8);">SP</div></a>';
          var gradients = ['linear-gradient(135deg,#fff8e1,#ffe082)', 'linear-gradient(135deg,#e8f5e9,#c8e6c9)', 'linear-gradient(135deg,#ffebee,#ffcdd2)'];
          function cardLg(p) {
            var cat = p.categorySlug || 'san-pham', url = base + '/' + esc(cat) + '/' + esc(p.slug);
            var img = p.primaryImageUrl ? '<img src="' + esc(p.primaryImageUrl) + '" alt="' + esc(p.primaryImageAlt || p.name) + '" class="ac-promo-card__img-lg" loading="lazy" />' : '<div class="ac-promo-card__img-lg" style="background:linear-gradient(135deg,#eec976,#9a7e2e);min-height:200px;display:flex;align-items:center;justify-content:center;color:rgba(255,255,255,0.8);font-size:2rem;">' + esc((p.name || '').substring(0, 2)) + '</div>';
            return '<a href="' + url + '" class="ac-promo-card ac-promo-card--lg" data-reveal><div class="ac-promo-card__content"><img src="' + base + '/logo/logo.png" alt="Tân Huê Viên" class="ac-promo-card__brand-logo" /><p class="ac-promo-card__sub">Đặc sản Sóc Trăng</p><h2 class="ac-promo-card__big">' + esc(p.name) + '</h2><p class="ac-promo-card__desc">' + esc(p.shortDescription || 'Sản phẩm chất lượng từ thương hiệu uy tín.') + '</p></div>' + img + '</a>';
          }
          function cardSm(p, i) {
            var cat = p.categorySlug || 'san-pham', url = base + '/' + esc(cat) + '/' + esc(p.slug), bg = gradients[i % 3];
            var label = p.isNew ? '<p class="ac-promo-card__label">Mới</p>' : (p.isPromotion ? '<p class="ac-promo-card__label">Khuyến mãi</p>' : '');
            var img = p.primaryImageUrl ? '<img src="' + esc(p.primaryImageUrl) + '" alt="' + esc(p.primaryImageAlt || p.name) + '" class="ac-promo-card__img" loading="lazy" />' : '<div class="ac-promo-card__img" style="background:rgba(255,255,255,0.2);min-height:120px;display:flex;align-items:center;justify-content:center;color:rgba(0,0,0,0.4);font-weight:bold;">' + esc((p.name || '').substring(0, 2)) + '</div>';
            return '<a href="' + url + '" class="ac-promo-card ac-promo-card--sm" data-reveal data-reveal-delay="' + ((i+1)*100) + '" style="background:' + bg + '"><div class="ac-promo-card__top">' + label + '<h3 class="ac-promo-card__title">' + esc(p.name) + '</h3></div>' + img + '</a>';
          }
          function renderProducts(products) {
            var $el = $('#home-featured-products'); if (!$el.length) return;
            if (!products || !products.length) { $el.html(fallbackProduct); return; }
            var html = cardLg(products[0]);
            $.each(products.slice(1, 4), function(i, p) { html += cardSm(p, i); });
            $el.html(html);
          }
          function renderPosts(posts) {
            var $el = $('#home-recent-posts'); if (!$el.length) return;
            if (!posts || !posts.length) { $el.html('<a href="' + base + '/bai-viet" class="ac-news-all-link">Xem tất cả tin tức →</a>'); return; }
            var f = posts[0], dStr = f.publishedAt ? new Date(f.publishedAt).toLocaleDateString('vi-VN') : '', imgStyle = f.featuredImageUrl ? '' : 'background:linear-gradient(135deg,#c9a23e,#6d5a10)';
            var html = '<article class="ac-news-featured" data-reveal-x style="--reveal-x:-40px"><a href="' + base + '/' + esc(f.slug) + '" class="ac-news-featured__img-link"><div class="ac-news-featured__img" style="' + imgStyle + '">' + (f.featuredImageUrl ? '<img src="' + esc(f.featuredImageUrl) + '" alt="' + esc(f.featuredImageAlt || f.title) + '" style="width:100%;height:100%;object-fit:cover;position:absolute;inset:0" />' : '') + '<div class="ac-news-featured__overlay"><span class="ac-news-cat ac-news-cat--green">' + esc(f.categoryName) + '</span><h3 class="ac-news-featured__title">' + esc(f.title) + '</h3></div></div></a><div class="ac-news-featured__meta"><time>' + esc(dStr) + '</time><a href="' + base + '/' + esc(f.slug) + '" class="ac-news-featured__read">Đọc tiếp →</a></div></article><div class="ac-news-list">';
            $.each(posts.slice(1), function(_, post) {
              var d = post.publishedAt ? new Date(post.publishedAt).toLocaleDateString('vi-VN') : '', istyle = post.featuredImageUrl ? 'transparent' : 'linear-gradient(135deg,#c9a23e,#eec976)';
              html += '<article class="ac-news-item" data-reveal><a href="' + base + '/' + esc(post.slug) + '" class="ac-news-item__img" style="background:' + istyle + ';position:relative;overflow:hidden">' + (post.featuredImageUrl ? '<img src="' + esc(post.featuredImageUrl) + '" alt="" style="width:100%;height:100%;object-fit:cover;position:absolute;inset:0" />' : '') + '</a><div class="ac-news-item__body"><div class="ac-news-item__meta"><time>' + esc(d) + '</time><span class="ac-news-cat ac-news-cat--green">' + esc(post.categoryName) + '</span></div><h3 class="ac-news-item__title"><a href="' + base + '/' + esc(post.slug) + '">' + esc(post.title) + '</a></h3></div></article>';
            });
            $el.html(html + '<a href="' + base + '/bai-viet" class="ac-news-all-link">Xem tất cả tin tức →</a></div>');
          }
          $.getJSON(base + '/api/products?page=1&pageSize=8&sort=newest').done(function(d) { renderProducts(d.items || []); }).fail(function() { renderProducts([]); });
          $.getJSON(base + '/api/posts?page=1&pageSize=4').done(function(d) { renderPosts(d.items || []); }).fail(function() { renderPosts([]); });

          $('#home-contact-form').on('submit', function(e) {
            e.preventDefault();
            var $form = $(this), $result = $('#home-contact-result'), $btn = $('#home-contact-submit');
            var fields = {};
            $form.find('[name]').each(function() { var n = $(this).attr('name'); if (n) fields[n] = ($(this).val() || '').trim(); });
            if (!fields.hoTen || !fields.phone) { $result.html('<span class="text-danger">Vui lòng điền Họ tên và Số điện thoại.</span>'); return false; }
            $result.empty();
            $btn.prop('disabled', true);
            $.ajax({ url: base + '/api/contact', method: 'POST', contentType: 'application/json', data: JSON.stringify({ subject: 'Liên hệ từ trang chủ', fields: fields }) })
              .done(function(res) { $result.html('<span class="text-success">✓ ' + (res.message || 'Đã gửi! Chúng tôi sẽ liên hệ lại sớm nhất.') + '</span>'); $form[0].reset(); })
              .fail(function(xhr) { var msg = (xhr.responseJSON && xhr.responseJSON.message) ? xhr.responseJSON.message : 'Gửi thất bại. Vui lòng thử lại.'; $result.html('<span class="text-danger">' + $('<div>').text(msg).html() + '</span>'); })
              .always(function() { $btn.prop('disabled', false); });
            return false;
          });
        })();
        """;
}
