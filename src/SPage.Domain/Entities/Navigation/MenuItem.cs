namespace SPage.Domain.Entities.Navigation;

public sealed class MenuLocation
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // Display name: "Header", "Footer"...
    public string Key { get; set; } = string.Empty;    // Unique slug: "header", "footer"...
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public ICollection<MenuItem> MenuItems { get; set; } = [];
}

public sealed class MenuItem
{
    public int Id { get; set; }

    public int LocationId { get; set; }
    public MenuLocation? Location { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public int? ParentId { get; set; }
    public MenuItem? Parent { get; set; }
    public ICollection<MenuItem> Children { get; set; } = [];

    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool OpenInNewTab { get; set; } = false;
}
