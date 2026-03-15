using SPage.Domain.Common;

namespace SPage.Domain.Entities.Store;

public enum StoreLocationType
{
    Dealer = 0,        // Đại lý
    Supermarket = 1,   // Siêu thị
    Showroom = 2,
    Office = 3
}

public enum StoreRegionType
{
    Domestic = 0,      // Trong nước
    International = 1  // Ngoài nước
}

public sealed class StoreLocation : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Địa chỉ đầy đủ</summary>
    public string FullAddress { get; set; } = string.Empty;
    public string? ProvinceOrCity { get; set; }
    public string? Country { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? MapLink { get; set; }

    /// <summary>Điện thoại bàn</summary>
    public string? LandlinePhone { get; set; }

    /// <summary>Di động</summary>
    public string? MobilePhone { get; set; }

    public string? ManagerName { get; set; }

    public StoreLocationType LocationType { get; set; } = StoreLocationType.Dealer;
    public StoreRegionType RegionType { get; set; } = StoreRegionType.Domestic;

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}

