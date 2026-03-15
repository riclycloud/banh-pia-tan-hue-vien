using Microsoft.AspNetCore.Mvc;
using SPage.Application.Common.Interfaces;

namespace SPage.ViewComponents;

public sealed class SiteHeadSnippetViewComponent : ViewComponent
{
    private readonly ISiteSettingsService _settings;

    public SiteHeadSnippetViewComponent(ISiteSettingsService settings)
    {
        _settings = settings;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var s = await _settings.GetAsync();
        return View(s);
    }
}

