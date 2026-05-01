using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>Controller تسجيل الدخول والخروج للوحة التحكم</summary>
public class AccountController(IMediator mediator) : Controller
{
    // ─── عرض صفحة تسجيل الدخول ────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (HttpContext.Session.GetString("AdminId") is not null)
            return RedirectToAction("Index", "Dashboard");

        ViewBag.ReturnUrl = returnUrl;
        return View(new AdminLoginDto());
    }

    // ─── معالجة تسجيل الدخول ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await mediator.Send(new AdminLoginCommand(dto));
        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "اسم المستخدم أو كلمة المرور غير صحيحة");
            return View(dto);
        }

        HttpContext.Session.SetString("AdminId",       result.UserId.ToString());
        HttpContext.Session.SetString("AdminUsername", result.Username);
        HttpContext.Session.SetString("AdminFullName", result.FullName);
        HttpContext.Session.SetString("AdminToken",    result.Token);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    // ─── تسجيل الخروج ─────────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    // ─── صفحة رفض الوصول ──────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult AccessDenied() => View();
}
