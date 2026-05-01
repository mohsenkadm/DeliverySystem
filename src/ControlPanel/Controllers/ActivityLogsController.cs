using DeliverySystem.Application.Features.ActivityLogs.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>Controller سجل النشاطات</summary>
public class ActivityLogsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(string? search, DateTime? from, DateTime? to)
    {
        var logs = await mediator.Send(new GetActivityLogsQuery(
            Action:      search,
            PerformedBy: null,
            From:        from,
            To:          to));
        ViewBag.Search = search;
        return View(logs);
    }
}
