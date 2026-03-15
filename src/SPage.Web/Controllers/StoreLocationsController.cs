using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using SPage.Application.Common.Interfaces;
using SPage.Domain.Entities.Store;
using SPage.ViewModels.Seo;

namespace SPage.Controllers;

/// <summary>Trang công khai: địa chỉ đại lý / cửa hàng, nhóm theo khu vực và loại hình.</summary>
public sealed class StoreLocationsController : Controller
{
    private readonly IApplicationDbContext _context;

    public StoreLocationsController(IApplicationDbContext context) => _context = context;

    [OutputCache(PolicyName = "PageDetail")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var locations = await _context.StoreLocations
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.RegionType)
            .ThenBy(x => x.LocationType)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        // Nhóm theo RegionType, rồi theo LocationType
        var groups = new List<StoreLocationGroupVm>();

        foreach (StoreRegionType region in Enum.GetValues(typeof(StoreRegionType)))
        {
            var inRegion = locations.Where(x => x.RegionType == region).ToList();
            if (inRegion.Count == 0) continue;

            var regionTitle = region == StoreRegionType.Domestic ? "Trong nước" : "Quốc tế";
            var subGroups = inRegion
                .GroupBy(x => x.LocationType)
                .OrderBy(g => g.Key)
                .Select(g => new StoreLocationSubGroupVm
                {
                    LocationType = g.Key,
                    LocationTypeLabel = GetLocationTypeLabel(g.Key),
                    Locations = g.ToList()
                })
                .ToList();

            groups.Add(new StoreLocationGroupVm
            {
                RegionType = region,
                Title = regionTitle,
                SubGroups = subGroups
            });
        }

        ViewData["Seo"] = new SeoViewModel
        {
            Title = "Đại lý & Địa chỉ cửa hàng",
            MetaDescription = "Danh sách đại lý, showroom và điểm bán hàng Tân Huê Viên trong nước và quốc tế.",
            CanonicalUrl = Url.Action("Index", "StoreLocations", null, Request.Scheme),
            IsIndexable = true
        };

        return View(groups);
    }

    private static string GetLocationTypeLabel(StoreLocationType type) => type switch
    {
        StoreLocationType.Showroom => "Showroom",
        StoreLocationType.Dealer => "Đại lý",
        StoreLocationType.Supermarket => "Siêu thị",
        StoreLocationType.Office => "Văn phòng",
        _ => type.ToString()
    };
}

public sealed class StoreLocationGroupVm
{
    public StoreRegionType RegionType { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<StoreLocationSubGroupVm> SubGroups { get; set; } = new();
}

public sealed class StoreLocationSubGroupVm
{
    public StoreLocationType LocationType { get; set; }
    public string LocationTypeLabel { get; set; } = string.Empty;
    public List<StoreLocation> Locations { get; set; } = new();
}
