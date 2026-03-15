using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Seo;
using SPage.Infrastructure.Persistence;

namespace SPage.Infrastructure.Services;

public sealed class SiteSettingsService : ISiteSettingsService
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;
    private static readonly string CacheKey = "site_settings";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public SiteSettingsService(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<SiteSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out SiteSettings? cached) && cached is not null)
        {
            return cached;
        }

        var settings = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            settings = new SiteSettings();
            _db.SiteSettings.Add(settings);
            await _db.SaveChangesAsync(cancellationToken);
        }

        _cache.Set(CacheKey, settings, CacheDuration);
        return settings;
    }

    public async Task UpdateAsync(SiteSettings settings, CancellationToken cancellationToken = default)
    {
        var existing = await _db.SiteSettings.FirstOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            _db.SiteSettings.Add(settings);
        }
        else
        {
            existing.SiteName = settings.SiteName;
            existing.SiteTagline = settings.SiteTagline;
            existing.DefaultMetaDescription = settings.DefaultMetaDescription;
            existing.PhoneNumber = settings.PhoneNumber;
            existing.Email = settings.Email;
            existing.Address = settings.Address;
            existing.FacebookPageUrl = settings.FacebookPageUrl;
            existing.GoogleAnalyticsId = settings.GoogleAnalyticsId;
            existing.CustomHeadHtml = settings.CustomHeadHtml;
            existing.CustomFooterHtml = settings.CustomFooterHtml;
            existing.ContactEmail = settings.ContactEmail;
            existing.HomePageId = settings.HomePageId;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _cache.Remove(CacheKey);
    }
}

