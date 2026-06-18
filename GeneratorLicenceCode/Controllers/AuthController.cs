using GeneratorLicenceCode.Helpers;
using GeneratorLicenceCode.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace GeneratorLicenceCode.Controllers;

public class AuthController : Controller
{
    private readonly AuthSettings _authSettings;

    public AuthController(IOptions<AuthSettings> authSettings)
    {
        _authSettings = authSettings.Value;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "License");

        ViewData["ReturnUrl"] = returnUrl;
        PrepareCaptcha();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!MathCaptchaHelper.Validate(HttpContext, model.MathAnswer))
        {
            ModelState.AddModelError(nameof(model.MathAnswer), "Güvenlik sorusunun cevabı hatalı.");
            PrepareCaptcha();
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            PrepareCaptcha();
            return View(model);
        }

        if (model.Username != _authSettings.Username || model.Password != _authSettings.Password)
        {
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
            PrepareCaptcha();
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "License");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private void PrepareCaptcha()
    {
        ViewData["MathQuestion"] = MathCaptchaHelper.GenerateQuestion(HttpContext);
    }
}
