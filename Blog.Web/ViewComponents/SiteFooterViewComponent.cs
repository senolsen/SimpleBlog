using Blog.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.ViewComponents;

public class SiteFooterViewComponent : ViewComponent
{
    private readonly ISiteSettingsService _siteSettingsService;

    public SiteFooterViewComponent(ISiteSettingsService siteSettingsService)
    {
        _siteSettingsService = siteSettingsService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var settings = await _siteSettingsService.GetSettingsAsync();
        return View(settings);
    }
}
