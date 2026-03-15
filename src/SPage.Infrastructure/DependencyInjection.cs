using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SPage.Application.Common;
using SPage.Application.Common.Interfaces;
using SPage.Infrastructure.Identity;
using SPage.Infrastructure.Caching;
using SPage.Infrastructure.Persistence;
using SPage.Infrastructure.Persistence.Interceptors;
using SPage.Infrastructure.Services;
using SPage.Infrastructure.Configuration;

namespace SPage.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database ─────────────────────────────────────────────────
        services.AddSingleton<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(maxRetryCount: 3);
                });
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(
            sp => sp.GetRequiredService<ApplicationDbContext>());

        // ── Identity ─────────────────────────────────────────────────
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.SignIn.RequireConfirmedEmail = false; // true trong production
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // ── Redis Cache ───────────────────────────────────────────────
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "spage:";
            });
        }
        else
        {
            // Fallback: In-Memory cache khi chưa có Redis
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, RedisCacheService>();

        // ── Services ─────────────────────────────────────────────────
        services.AddScoped<ISlugService, SlugService>();
        services.AddScoped<ISlugLookupService, SlugLookupService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<ISitemapService, SitemapService>();
        services.AddScoped<IRobotsTxtService, RobotsTxtService>();
        services.AddScoped<ISlugHistoryService, SlugHistoryService>();
        services.AddScoped<ISiteSettingsService, SiteSettingsService>();
        services.AddScoped<IEmailConfigService, EmailConfigService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ISeoAiService, SPage.Infrastructure.Services.SeoAi.SeoAiService>();

        services.AddHttpClient();

        services.Configure<EmailSettings>(configuration.GetSection("Email"));

        services.AddMemoryCache();          // IMemoryCache cho SlugHistoryService & SiteSettingsService
        services.AddHttpContextAccessor();

        return services;
    }
}
