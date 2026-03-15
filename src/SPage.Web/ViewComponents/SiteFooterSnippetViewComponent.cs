using Microsoft.AspNetCore.Mvc;
using SPage.Application.Common.Interfaces;

namespace SPage.ViewComponents;

public sealed class SiteFooterSnippetViewComponent : ViewComponent
{
    private readonly ISiteSettingsService _settings;

    public SiteFooterSnippetViewComponent(ISiteSettingsService settings)
    {
        _settings = settings;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
    {
        var s = await _settings.GetAsync(cancellationToken);
        return View(s);
    }
}

