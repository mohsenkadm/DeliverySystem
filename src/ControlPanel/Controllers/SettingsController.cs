using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Settings.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

public class SettingsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index()
    {
        var settings = await mediator.Send(new GetSystemSettingsQuery());
        return View(settings);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SystemSettingsDto dto, IFormFile? logo)
    {
        if (logo != null && logo.Length > 0)
        {
            var dir = Path.Combine("wwwroot", "uploads", "system");
            Directory.CreateDirectory(dir);
            var fn = $"logo{Path.GetExtension(logo.FileName)}";
            using var fs = new FileStream(Path.Combine(dir, fn), FileMode.Create);
            await logo.CopyToAsync(fs);
            dto.LogoPath = $"/uploads/system/{fn}";
        }
        await mediator.Send(new UpdateSystemSettingsCommand(dto));
        TempData["Success"] = "تم حفظ إعدادات النظام";
        return RedirectToAction(nameof(Index));
    }
}
