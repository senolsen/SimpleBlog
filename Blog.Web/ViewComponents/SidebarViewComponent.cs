using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.ViewComponents;

public class SidebarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
