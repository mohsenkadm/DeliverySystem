using DeliverySystem.Application.Features.Notifications.Commands;
using DeliverySystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

public class NotificationsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(int page = 1)
    {
        var notifs = await mediator.Send(new GetAllNotificationsAdminQuery(page, 50));
        ViewBag.Page = page;
        return View(notifs);
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string title, string body, int target, int? targetUserId)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            TempData["Error"] = "العنوان والمحتوى مطلوبان";
            return RedirectToAction(nameof(Index));
        }
        await mediator.Send(new SendBulkNotificationCommand(title, body, (NotificationTarget)target, targetUserId));
        TempData["Success"] = "تم إرسال الإشعار بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarkRead(int id)
    {
        await mediator.Send(new MarkNotificationReadCommand(id));
        return Ok();
    }
}
