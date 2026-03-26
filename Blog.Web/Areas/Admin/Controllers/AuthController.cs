using Blog.Core.Entities;
using Blog.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Web.Areas.Admin.Controllers;

[Area("Admin")]
[AllowAnonymous] // Herkes bu sayfaya erişebilmeli ki giriş yapabilsin
public class AuthController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Eğer kullanıcı zaten giriş yapmışsa direkt Dashboard'a at
        if (User.Identity!.IsAuthenticated)
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                // lockoutOnFailure parametresini TRUE yaptık!
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                if (result.IsLockedOut) // KİLİTLENME DURUMUNU YAKALIYORUZ
                {
                    ModelState.AddModelError(string.Empty, "Çok fazla yanlış deneme yaptınız. Lütfen 15 dakika sonra tekrar deneyin.");
                    return View(model);
                }
            }

            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı!");
        }
        return View(model);
    }
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Auth", new { area = "Admin" });
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}