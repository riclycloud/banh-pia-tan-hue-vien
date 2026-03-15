using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SPage;
using SPage.Application;
using SPage.Infrastructure;
using SPage.Infrastructure.Persistence;
using SPage.Application.Common.Interfaces;
using SPage.Middlewares;
using SPage.Web.SeedData;
using SPage.Web.Startup;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────────────────────

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// ★ Localization — đăng ký resource files
builder.Services.AddLocalization(options =>
    options.ResourcesPath = "Resources");

// ★ AntiForgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// ★ Application Layer (MediatR, FluentValidation, Behaviours)
builder.Services.AddApplicationServices();

// ★ Infrastructure Layer (EF Core, Identity, Redis, Services)
builder.Services.AddInfrastructureServices(builder.Configuration);

// ★ Output Cache .NET 8 — named policies
// Đọc cấu hình OutputCache:Enabled từ appsettings.json (mặc định: true)
var outputCacheEnabled = builder.Configuration.GetValue<bool>("OutputCache:Enabled", true);
TimeSpan CacheMins(int m)  => outputCacheEnabled ? TimeSpan.FromMinutes(m) : TimeSpan.Zero;
TimeSpan CacheHours(int h) => outputCacheEnabled ? TimeSpan.FromHours(h)   : TimeSpan.Zero;

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("PostsList", policy =>
        policy.Expire(CacheMins(10))
              .SetVaryByQuery("page", "category", "tag", "q")
              .Tag("posts", "posts-list"));

    options.AddPolicy("PostDetail", policy =>
        policy.Expire(CacheMins(30))
              .SetVaryByRouteValue("slug")
              .Tag("posts", "posts-detail"));

    options.AddPolicy("ProductsList", policy =>
        policy.Expire(CacheMins(10))
              .SetVaryByQuery("page", "category", "q", "sort")
              .Tag("products"));

    options.AddPolicy("PageDetail", policy =>
        policy.Expire(CacheHours(1))
              .SetVaryByRouteValue("slug")
              .Tag("pages"));

    options.AddPolicy("VideosList", policy =>
        policy.Expire(CacheMins(10))
              .SetVaryByQuery("categoryId", "q", "page")
              .Tag("videos", "videos-list"));

    options.AddPolicy("VideoDetail", policy =>
        policy.Expire(CacheMins(60))
              .SetVaryByRouteValue("slug")
              .Tag("videos", "videos-detail"));

    // Sitemap: cache 1 giờ, tag để có thể purge khi cập nhật nội dung
    options.AddPolicy("Sitemap", policy =>
        policy.Expire(CacheHours(1))
              .Tag("sitemap"));
});

// ★ Response Caching (HTTP cache headers)
builder.Services.AddResponseCaching();

// ★ Cấu hình Localization — supported cultures
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("vi"),
        new CultureInfo("vi-VN"),
        new CultureInfo("en"),
        new CultureInfo("en-US"),
        new CultureInfo("en-GB")
    };

    options.DefaultRequestCulture = new RequestCulture("vi", "vi");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// ★ Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// ★ Cookie authentication settings (Identity)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

var app = builder.Build();

//// ── Tạo schema từ model (không dùng migration) + seed data ─────────────────────
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
//    // Tạo database và toàn bộ bảng từ model nếu chưa tồn tại (AspNetRoles, AspNetUsers, SiteSettings, ...)
//    var created = await db.Database.EnsureCreatedAsync();
//    if (created)
//        logger.LogInformation("Database created from model (EnsureCreated).");

//    // Đảm bảo bảng SlugLookups tồn tại (phòng trường hợp DB cũ thiếu bảng)
//    await EnsureSlugLookupsTable.EnsureSlugLookupsTableAsync(db);

//    try
//    {
//        // Seed: tạo tài khoản admin mặc định nếu chưa có
//        await DataSeeder.SeedAdminUserAsync(scope.ServiceProvider);

//        // Seed dữ liệu mẫu (category, bài viết, sản phẩm, cửa hàng...)
//        await DataSeeder.SeedSampleDataAsync(scope.ServiceProvider);

//        // Seed default menu locations (idempotent)
//        await DataSeeder.SeedMenuLocationsAsync(scope.ServiceProvider);

//        // Seed dynamic pages (xóa + tạo lại để cập nhật nội dung)
//        await DataSeeder.SeedPagesAsync(scope.ServiceProvider);
//        await DataSeeder.SeedHomePageAsync(scope.ServiceProvider);

//        // Seed contact form fields cho form đăng ký tham quan
//        await DataSeeder.SeedContactFieldsAsync(scope.ServiceProvider);

//        // Đồng bộ SlugLookups từ Post, Category, ProductCategory, Product (tự tạo DB + set data slug)
//        await DataSeeder.SeedSlugLookupsAsync(scope.ServiceProvider);
//    }
//    catch (DbUpdateException ex)
//    {
//        var inner = ex.InnerException?.Message ?? ex.Message;
//        logger.LogError(ex, "Seed/DB update failed: {Inner}", inner);
//        throw new InvalidOperationException($"Lỗi cập nhật database khi seed: {inner}", ex);
//    }
//}

// ── Middleware Pipeline ──────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

// ★ Static Files với Long-term Cache Headers
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".webp") ||
            ctx.File.Name.EndsWith(".css") ||
            ctx.File.Name.EndsWith(".js"))
        {
            ctx.Context.Response.Headers.CacheControl =
                "public, max-age=31536000, immutable";
        }
    }
});

// ★ URL-based culture + cookie-based localization
app.UseMiddleware<LocalizationCultureMiddleware>();
app.UseRequestLocalization(app.Services
    .GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// ★ Trang 404 (và 4xx/5xx) — re-execute về Home/StatusCode, view style trang chủ
app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

// ★ Smart SEO Redirect (404 → slug history lookup → 301)
app.UseMiddleware<SmartSeoRedirectMiddleware>();

// ★ URL normalization (trailing slash, uppercase→lowercase)
app.UseMiddleware<SeoRedirectMiddleware>();

// ★ Slug rewrite (không redirect): /bai-viet-1 → xử lý nội bộ như /Posts/Detail?slug=...
app.UseMiddleware<SlugRewriteMiddleware>();

app.UseResponseCaching();
// Chỉ bật Output Cache middleware khi appsettings OutputCache:Enabled = true
if (outputCacheEnabled)
    app.UseOutputCache();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ★ Sitemap.xml — động từ DB, chuẩn SEO (lastmod ISO8601, priority, changefreq), cache 1h
app.MapGet("/sitemap.xml", async (ISitemapService svc, CancellationToken ct) =>
    Results.Content(await svc.GenerateAsync(ct), "application/xml; charset=utf-8"))
    .WithName("Sitemap")
    .CacheOutput("Sitemap");

// ★ Robots.txt — chuẩn RFC/sitemaps.org, Sitemap URL tuyệt đối, có thể override bằng Seo:RobotsContent
app.MapGet("/robots.txt", async (IRobotsTxtService robots, CancellationToken ct) =>
{
    var content = await robots.GetContentAsync(ct);
    return Results.Text(content, "text/plain; charset=utf-8");
})
    .WithName("RobotsTxt")
    .CacheOutput("Sitemap");

// ★ Routes (thứ tự từ specific → generic)
app.MapControllerRoute("language-culture",
    "{culture:regex(^(vi|en)$)}/{controller=Home}/{action=Index}/{id?}");

// ★ Backward-compat: /blog/{slug} → /{slug} (301) — giữ SEO juice cũ
app.MapGet("/blog/{slug}", (string slug) =>
    Results.Redirect($"/{slug}", permanent: true));

// ★ Video Library routes — /video/{slug} ưu tiên danh mục (slug category), không có thì slug video
app.MapControllerRoute("video-byslug", "video/{slug}",
    new { controller = "Video", action = "BySlug" });
app.MapControllerRoute("video-index", "video",
    new { controller = "Video", action = "Index" });

// {categorySlug}/{productSlug} — constraint tránh trùng tên controller/route
app.MapControllerRoute("product-detail",
    "{categorySlug}/{productSlug}",
    new { controller = "Products", action = "Detail" },
    new { categorySlug = new RegexRouteConstraint(new Regex("^(?!home$|posts$|products$|account$|admin$|contact$|video$|api$|san-pham$|bai-viet$|lien-he$|dai-ly$|blog$)[a-z0-9-]+$")) });
// Fallback: sản phẩm không có danh mục hoặc link cũ /san-pham/{slug}
app.MapControllerRoute("product-detail-fallback",
    "san-pham/{productSlug}",
    new { controller = "Products", action = "Detail", categorySlug = "san-pham" });
app.MapControllerRoute("products",
    "san-pham", new { controller = "Products", action = "Index" });
app.MapControllerRoute("posts",
    "bai-viet", new { controller = "Posts", action = "Index" });
app.MapControllerRoute("store-locations",
    "dai-ly", new { controller = "StoreLocations", action = "Index" });
app.MapControllerRoute("contact",
    "lien-he", new { controller = "Contact", action = "Index" });
app.MapControllerRoute("account",
    "account/{action}", new { controller = "Account" });
app.MapControllerRoute("admin",
    "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute("default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();
