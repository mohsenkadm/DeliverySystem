using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Invoices.Commands;
using DeliverySystem.Application.Features.Customers.Queries;
using DeliverySystem.Application.Features.Employees.Commands;
using DeliverySystem.Application.Features.Products.Commands;
using DeliverySystem.Application.Features.ActivityLogs.Commands;
using DeliverySystem.Domain.Enums;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliverySystem.ControlPanel.Controllers;

/// <summary>Controller إدارة الفواتير</summary>
public class InvoicesController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(string? invoiceNumber, int? customerId, int? status)
    {
        var invoiceStatus = status.HasValue ? (InvoiceStatus?)status.Value : null;
        var invoices = await mediator.Send(new GetAllInvoicesQuery(invoiceNumber, customerId, Status: invoiceStatus));

        ViewBag.Customers = await mediator.Send(new GetAllCustomersQuery());
        ViewBag.TotalAmount    = invoices.Sum(i => i.TotalAmount);
        ViewBag.TotalPaid      = invoices.Sum(i => i.PaidAmount);
        ViewBag.TotalRemaining = invoices.Sum(i => i.RemainingAmount);

        return View(invoices);
    }

    public async Task<IActionResult> Details(int id)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id));
        if (invoice is null) return NotFound();
        ViewBag.Drivers = await mediator.Send(new GetAllEmployeesQuery());
        return View(invoice);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Customers = await mediator.Send(new GetAllCustomersQuery());
        ViewBag.Drivers   = await mediator.Send(new GetAllEmployeesQuery());
        ViewBag.Products  = await mediator.Send(new GetAllProductsQuery());
        return View(new CreateInvoiceDto());
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInvoiceDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await mediator.Send(new GetAllCustomersQuery());
            ViewBag.Drivers   = await mediator.Send(new GetAllEmployeesQuery());
            ViewBag.Products  = await mediator.Send(new GetAllProductsQuery());
            return View(dto);
        }
        var result = await mediator.Send(new CreateInvoiceCommand(dto));
        await mediator.Send(new LogActivityCommand("إنشاء فاتورة", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم إنشاء الفاتورة {result.InvoiceNumber}"));
        TempData["Success"] = $"تم إنشاء الفاتورة {result.InvoiceNumber} بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Accept(int id)
    {
        await mediator.Send(new UpdateInvoiceStatusCommand(id, InvoiceStatus.Accepted));
        await mediator.Send(new LogActivityCommand("قبول فاتورة", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم قبول الفاتورة رقم {id}"));
        TempData["Success"] = "تم قبول الفاتورة";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id)
    {
        await mediator.Send(new UpdateInvoiceStatusCommand(id, InvoiceStatus.Rejected));
        await mediator.Send(new LogActivityCommand("رفض فاتورة", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم رفض الفاتورة رقم {id}"));
        TempData["Success"] = "تم رفض الفاتورة";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Defer(int id)
    {
        await mediator.Send(new UpdateInvoiceStatusCommand(id, InvoiceStatus.Deferred));
        await mediator.Send(new LogActivityCommand("تأجيل فاتورة", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم تأجيل الفاتورة رقم {id}"));
        TempData["Success"] = "تم تأجيل الفاتورة";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        await mediator.Send(new UpdateInvoiceStatusCommand(id, InvoiceStatus.Completed));
        TempData["Success"] = "تم إكمال الفاتورة";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AssignDriver(int id, int employeeId)
    {
        await mediator.Send(new AssignDriverToInvoiceCommand(id, employeeId));
        await mediator.Send(new LogActivityCommand("تعيين سائق", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم تعيين سائق للفاتورة رقم {id}"));
        TempData["Success"] = "تم تعيين السائق وتحويل الفاتورة للتوصيل";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> StartWarehouseProcessing(int id)
    {
        var ok = await mediator.Send(new StartInvoiceWarehouseProcessingCommand(id));
        TempData[ok ? "Success" : "Error"] = ok ? "بدأ المستودع بتجهيز الطلب" : "لا يمكن تغيير الحالة في الوقت الحالي";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> DispatchDriver(int id, int employeeId)
    {
        var ok = await mediator.Send(new AssignDriverAndDispatchCommand(id, employeeId));
        TempData["Success"] = "تم إرسال الطلب مع السائق للتوصيل";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> PayPartial(int id, decimal amount)
    {
        if (amount <= 0) { TempData["Error"] = "المبلغ يجب أن يكون أكبر من صفر"; return RedirectToAction(nameof(Details), new { id }); }
        await mediator.Send(new PayInvoiceCommand(id, amount));
        await mediator.Send(new LogActivityCommand("دفعة جزئية", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم تسجيل دفعة {amount:N2} د.ع للفاتورة {id}"));
        TempData["Success"] = $"تم تسجيل دفعة {amount:N2} د.ع";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> PayFull(int id)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id));
        if (invoice is null) return NotFound();
        await mediator.Send(new PayInvoiceCommand(id, invoice.RemainingAmount));
        await mediator.Send(new LogActivityCommand("دفع كامل", HttpContext.Session.GetString("AdminFullName") ?? "مجهول", "إدارة", $"تم تسجيل الدفع الكامل للفاتورة {id}"));
        TempData["Success"] = "تم تسجيل الدفع الكامل للفاتورة";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportExcel()
    {
        var invoices = await mediator.Send(new GetAllInvoicesQuery());
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("الفواتير");
        ws.Cell(1,1).Value="رقم الفاتورة"; ws.Cell(1,2).Value="التاريخ";
        ws.Cell(1,3).Value="العميل";       ws.Cell(1,4).Value="السائق";
        ws.Cell(1,5).Value="الإجمالي";     ws.Cell(1,6).Value="المدفوع";
        ws.Cell(1,7).Value="المتبقي";      ws.Cell(1,8).Value="الحالة";
        int row = 2;
        foreach (var i in invoices)
        {
            ws.Cell(row,1).Value = i.InvoiceNumber;
            ws.Cell(row,2).Value = i.OrderDate.ToString("yyyy/MM/dd");
            ws.Cell(row,3).Value = i.CustomerName;
            ws.Cell(row,4).Value = i.EmployeeName ?? "-";
            ws.Cell(row,5).Value = i.TotalAmount;
            ws.Cell(row,6).Value = i.PaidAmount;
            ws.Cell(row,7).Value = i.RemainingAmount;
            ws.Cell(row,8).Value = i.StatusText;
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream(); wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "الفواتير.xlsx");
    }
}
