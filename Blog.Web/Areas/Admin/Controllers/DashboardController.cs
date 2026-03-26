using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Editör,Yazar")] // Sistemin güvenliğini sağlıyoruz
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}