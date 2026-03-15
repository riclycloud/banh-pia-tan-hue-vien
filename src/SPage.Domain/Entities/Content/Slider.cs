namespace SPage.Domain.Entities.Content;

public enum SliderPosition
{
    HomeHero = 0,
    HomeBelowHero = 1,
    SidebarRight = 2,
    FooterBanner = 3
}

public sealed class Slider
{
    public int Id { get; set; }
    public SliderPosition Position { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Slide> Slides { get; set; } = [];
}

public sealed class Slide
{
    public int Id { get; set; }
    public int SliderId { get; set; }
    public Slider Slider { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public string? LinkUrl { get; set; }
    public bool OpenInNewTab { get; set; } = false;

    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

