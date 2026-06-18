using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeneratorLicenceCode.Controllers;

[Authorize]
public class GeneratorController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Create", "License");
    }
}
