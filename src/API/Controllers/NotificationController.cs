using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Notifications.Commands;
using DeliverySystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliverySystem.API.Controllers;

/// <summary>Controller إدارة الإشعارات</summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationController(IMediator mediator) : ControllerBase
{
    /// <summary>جلب إشعارات المستخدم الحالي</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationDto>>), 200)]
    public async Task<IActionResult> GetMyNotifications()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var target = role switch
        {
            "Admin" => NotificationTarget.Admin,
            "Driver" => NotificationTarget.Driver,
            "Representative" => NotificationTarget.Representative,
            _ => NotificationTarget.Customer
        };
        var result = await mediator.Send(new GetNotificationsQuery(target, userId));
        return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(result));
    }

    /// <summary>تحديد إشعار كمقروء</summary>
    [HttpPatch("{id:int}/read")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    public async Task<IActionResult> MarkRead(int id)
    {
        var result = await mediator.Send(new MarkNotificationReadCommand(id));
        if (!result) return NotFound(ApiResponse<bool>.Fail("الإشعار غير موجود", "Notification not found"));
        return Ok(ApiResponse<bool>.Ok(true, "تم تحديد الإشعار كمقروء", "Notification marked as read"));
    }
}
