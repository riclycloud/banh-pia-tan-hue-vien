using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Blog;
using SPage.Infrastructure.Identity;
using SPage.Domain.Entities.Page;
using SPage.Domain.Entities.Product;
using SPage.Domain.Entities.Seo;
using SPage.Domain.Entities.Navigation;
using SPage.Domain.Entities.Content;
using SPage.Domain.Entities.Contact;
using SPage.Domain.Entities.Store;
using SPage.Domain.Entities.Media;
using SPage.Domain.Entities.Video;
using SPage.Domain.Entities.Email;
using SPage.Domain.Entities.Ai;

namespace SPage.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<DynamicPage> DynamicPages => Set<DynamicPage>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<SlugHistory> SlugHistories => Set<SlugHistory>();
    public DbSet<SlugLookup> SlugLookups => Set<SlugLookup>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<MenuLocation> MenuLocations => Set<MenuLocation>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Slider> Sliders => Set<Slider>();
    public DbSet<Slide> Slides => Set<Slide>();
    public DbSet<ContactField> ContactFields => Set<ContactField>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<StoreLocation> StoreLocations => Set<StoreLocation>();
    public DbSet<SectionTemplate> SectionTemplates => Set<SectionTemplate>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<VideoEntity> Videos => Set<VideoEntity>();
    public DbSet<VideoCategory> VideoCategories => Set<VideoCategory>();
    public DbSet<EmailSendConfig> EmailSendConfigs => Set<EmailSendConfig>();
    public DbSet<RecipientEmail> RecipientEmails => Set<RecipientEmail>();
    public DbSet<AiProviderConfig> AiProviderConfigs => Set<AiProviderConfig>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
