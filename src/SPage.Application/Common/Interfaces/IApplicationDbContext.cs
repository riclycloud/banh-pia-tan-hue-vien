using Microsoft.EntityFrameworkCore;
using SPage.Domain.Entities.Blog;
using SPage.Domain.Entities.Media;
using SPage.Domain.Entities.Page;
using SPage.Domain.Entities.Product;
using SPage.Domain.Entities.Seo;
using SPage.Domain.Entities.Contact;
using SPage.Domain.Entities.Navigation;
using SPage.Domain.Entities.Content;
using SPage.Domain.Entities.Store;
using SPage.Domain.Entities.Video;
using SPage.Domain.Entities.Email;
using SPage.Domain.Entities.Ai;

namespace SPage.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Post> Posts { get; }
    DbSet<Category> Categories { get; }
    DbSet<Tag> Tags { get; }
    DbSet<DynamicPage> DynamicPages { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductAttribute> ProductAttributes { get; }
    DbSet<SlugHistory> SlugHistories { get; }
    DbSet<SlugLookup> SlugLookups { get; }
    DbSet<SiteSettings> SiteSettings { get; }
    DbSet<MenuLocation> MenuLocations { get; }
    DbSet<MenuItem> MenuItems { get; }
    DbSet<Slider> Sliders { get; }
    DbSet<Slide> Slides { get; }
    DbSet<ContactField> ContactFields { get; }
    DbSet<ContactMessage> ContactMessages { get; }
    DbSet<StoreLocation> StoreLocations { get; }
    DbSet<SectionTemplate> SectionTemplates { get; }
    DbSet<MediaFile> MediaFiles { get; }
    DbSet<VideoEntity> Videos { get; }
    DbSet<VideoCategory> VideoCategories { get; }
    DbSet<EmailSendConfig> EmailSendConfigs { get; }
    DbSet<RecipientEmail> RecipientEmails { get; }
    DbSet<AiProviderConfig> AiProviderConfigs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
