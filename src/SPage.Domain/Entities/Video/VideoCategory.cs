using SPage.Domain.Common;

namespace SPage.Domain.Entities.Video;

public sealed class VideoCategory : BaseAuditableEntity
{
    public string Name         { get; set; } = string.Empty;
    public string Slug         { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int    DisplayOrder { get; set; }
    public bool   IsActive     { get; set; } = true;

    public ICollection<VideoEntity> Videos { get; set; } = [];
}
