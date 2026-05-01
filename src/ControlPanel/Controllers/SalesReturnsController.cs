using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Employees.Commands;
using DeliverySystem.Application.Features.Invoices.Commands;
using DeliverySystem.Application.Features.Products.Commands;
using DeliverySystem.Application.Features.SalesReturns.Commands;
using DeliverySystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

public class SalesReturnsController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(int? status)
    {
        var s = status.HasValue ? (SalesReturnStatus?)status.Value : null;
        var returns = await mediator.Send(new GetAllSalesReturnsQuery(s));
        ViewBag.Status = status;
        return View(returns);
    }

    public async Task<IActionResult> Details(int id)
    {
        var ret = await mediator.Send(new GetSalesReturnByIdQuery(id));
        if (ret is null) return NotFound();
        return View(ret);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Employees = await mediator.Send(new GetAllEmployeesQuery());
        ViewBag.Invoices  = await mediator.Send(new GetAllInvoicesQuery());
        ViewBag.Products  = await mediator.Send(new GetAllProductsQuery());
        return View(new CreateSalesReturnDto());
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSalesReturnDto dto, IFormFile? photo)
    {
        var adminId = int.TryParse(HttpContext.Session.GetString("AdminId"), out var aid) ? aid : 1;

        if (photo != null && photo.Length > 0)
        {
            var dir = Path.Combine("wwwroot", "uploads", "returns");
            Directory.CreateDirectory(dir);
            var fn = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
            using var fs = new FileStream(Path.Combine(dir, fn), FileMode.Create);
            await photo.CopyToAsync(fs);
        }

        await mediator.Send(new CreateSalesReturnCommand(dto, adminId));
        TempData["Success"] = "تم إنشاء طلب المرتجع بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ApproveManager(int id)
    {
        var adminId = int.TryParse(HttpContext.Session.GetString("AdminId"), out var aid) ? aid : 1;
        var ok = await mediator.Send(new ApproveSalesReturnByManagerCommand(id, adminId));
        TempData[ok ? "Success" : "Error"] = ok ? "تمت الموافقة من المدير" : "لا يمكن تنفيذ هذا الإجراء";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> ApproveWarehouse(int id)
    {
        var ok = await mediator.Send(new ApproveSalesReturnByWarehouseCommand(id));
        TempData[ok ? "Success" : "Error"] = ok ? "تمت الموافقة من المستودع" : "لا يمكن تنفيذ هذا الإجراء";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var ok = await mediator.Send(new CompleteSalesReturnCommand(id));
        TempData[ok ? "Success" : "Error"] = ok ? "تم إكمال المرتجع وتحديث المخزون" : "لا يمكن تنفيذ هذا الإجراء";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var ok = await mediator.Send(new RejectSalesReturnCommand(id, reason));
        TempData[ok ? "Success" : "Error"] = ok ? "تم رفض طلب المرتجع" : "لا يمكن تنفيذ هذا الإجراء";
        return RedirectToAction(nameof(Details), new { id });
    }
}
