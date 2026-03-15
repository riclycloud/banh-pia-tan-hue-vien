namespace SPage.Application.Features.Menus.DTOs;

public sealed record MenuItemApiDto(
    string Title,
    string Url,
    bool OpenInNewTab,
    List<MenuItemApiDto> Children
);
