using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Payments.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

public class PaymentsController(IMediator mediator) : Controller
{
    // ── Index ─────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(bool? verified)
    {
        var list = await mediator.Send(new GetAllPaymentsQuery(IsVerified: verified));
        ViewData["Title"] = "سجل المدفوعات";
        ViewBag.VerifiedFilter = verified;
        return View(list);
    }

    // ── Verify (Cashier) ──────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Verify(int id)
    {
        var cashierId = int.Parse(HttpContext.Session.GetString("AdminId") ?? "0");
        var ok = await mediator.Send(new VerifyPaymentCommand(id, cashierId));
        TempData[ok ? "Success" : "Error"] = ok ? "تم التحقق من الدفعة وتسجيلها" : "الدفعة غير موجودة أو محققة مسبقاً";
        return RedirectToAction(nameof(Index));
    }
}
