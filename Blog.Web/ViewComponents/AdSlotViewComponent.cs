using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.ViewComponents;

public class AdSlotViewComponent : ViewComponent
{
    private readonly ISiteSettingsService _siteSettingsService;

    public AdSlotViewComponent(ISiteSettingsService siteSettingsService)
    {
        _siteSettingsService = siteSettingsService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string slot)
    {
        var settings = await _siteSettingsService.GetSettingsAsync();
        var code = slot?.ToLowerInvariant() switch
        {
            "sidebar" => settings.SidebarAdCode,
            "post-bottom" => settings.PostBottomAdCode,
            "home-list" => settings.HomeListAdCode,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(code))
            return Content(string.Empty);

        return View("Default", code);
    }
}
