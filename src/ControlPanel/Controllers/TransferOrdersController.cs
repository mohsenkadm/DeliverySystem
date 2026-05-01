using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.TransferOrders.Commands;
using DeliverySystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DeliverySystem.ControlPanel.Controllers;

public class TransferOrdersController(IMediator mediator) : Controller
{
    // ── Index ─────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(TransferOrderStatus? status, TransferOrderType? type)
    {
        var list = await mediator.Send(new GetAllTransferOrdersQuery(Status: status, OrderType: type));

        ViewData["Title"] = "طلبات النقل بين المستودعات";
        ViewBag.StatusFilter = status;
        ViewBag.TypeFilter = type;
        ViewBag.StatusList = Enum.GetValues<TransferOrderStatus>()
            .Select(s => new SelectListItem(StatusText(s), ((int)s).ToString(), s == status))
            .Prepend(new SelectListItem("جميع الحالات", "", !status.HasValue));
        return View(list);
    }

    // ── Details ───────────────────────────────────────────────────────────────

    public async Task<IActionResult> Details(int id)
    {
        var order = await mediator.Send(new GetTransferOrderByIdQuery(id));
        if (order is null) return NotFound();
        ViewData["Title"] = $"طلب نقل #{order.OrderNumber}";
        return View(order);
    }

    // ── Approve (Accountant) ──────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        var accountantId = int.Parse(HttpContext.Session.GetString("AdminId") ?? "0");
        var dto = new ApproveTransferOrderDto { ApprovedByEmployeeId = accountantId };
        var ok = await mediator.Send(new ApproveTransferOrderCommand(id, dto));
        TempData[ok ? "Success" : "Error"] = ok ? "تمت الموافقة على طلب النقل" : "لا يمكن الموافقة على هذا الطلب";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Start Warehouse Processing ─────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> StartProcessing(int id)
    {
        var ok = await mediator.Send(new StartWarehouseProcessingCommand(id));
        TempData[ok ? "Success" : "Error"] = ok ? "بدأ المستودع بمعالجة الطلب" : "لا يمكن تغيير حالة هذا الطلب";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Complete Transfer (updates inventory) ─────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var ok = await mediator.Send(new CompleteTransferOrderCommand(id));
        TempData[ok ? "Success" : "Error"] = ok ? "تم إكمال النقل وتحديث المخزون" : "لا يمكن إكمال هذا الطلب";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Reject ────────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Reject(int id)
    {
        var ok = await mediator.Send(new RejectTransferOrderCommand(id));
        TempData[ok ? "Success" : "Error"] = ok ? "تم رفض طلب النقل" : "خطأ في الرفض";
        return RedirectToAction(nameof(Index));
    }

    private static string StatusText(TransferOrderStatus s) => s switch
    {
        TransferOrderStatus.Pending             => "معلق",
        TransferOrderStatus.AccountantApproved  => "موافقة المحاسب",
        TransferOrderStatus.WarehouseProcessing => "يعالجه المستودع",
        TransferOrderStatus.Completed           => "مكتمل",
        TransferOrderStatus.Rejected            => "مرفوض",
        TransferOrderStatus.ReturnPending        => "طلب إرجاع معلق",
        TransferOrderStatus.ReturnApproved       => "موافقة الإرجاع",
        TransferOrderStatus.ReturnCompleted      => "اكتمل الإرجاع",
        _                                        => ""
    };
}
