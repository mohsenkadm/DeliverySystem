using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Invoices.Commands;
using DeliverySystem.Application.Features.Payments.Commands;
using DeliverySystem.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DeliverySystem.API.Controllers;

/// <summary>API مدير المبيعات</summary>
[ApiController]
[Route("api/mobile/manager")]
[Authorize(Roles = "SalesManager,Employee")]
[Produces("application/json")]
public class MobileManagerController(IMediator mediator, ApplicationDbContext db) : ControllerBase
{
    private int ManagerId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // ── المندوبون ─────────────────────────────────────────────────────────────

    /// <summary>جميع المندوبين</summary>
    [HttpGet("reps")]
    public async Task<IActionResult> GetAllReps()
    {
        var reps = await db.Employees
            .Where(e => e.IsActive && e.Roles != null && e.Roles.Contains("Representative"))
            .Select(e => new
            {
                e.Id, e.FullName, e.Phone, e.Region, e.EmployeeType,
                SupervisorName = e.Supervisor != null ? e.Supervisor.FullName : null,
                CustomerCount = db.Customers.Count(c => c.EmployeeId == e.Id),
                TotalInvoices = db.Invoices.Count(i => i.EmployeeId == e.Id),
                TotalSales = db.Invoices.Where(i => i.EmployeeId == e.Id).Sum(i => (decimal?)i.TotalAmount) ?? 0
            }).ToListAsync();
        return Ok(ApiResponse<object>.Ok(reps));
    }

    /// <summary>فواتير مندوب محدد</summary>
    [HttpGet("reps/{repId:int}/invoices")]
    public async Task<IActionResult> GetRepInvoices(int repId, [FromQuery] Domain.Enums.InvoiceStatus? status = null)
    {
        var result = await mediator.Send(new GetAllInvoicesQuery(EmployeeId: repId, Status: status));
        return Ok(ApiResponse<object>.Ok(result));
    }

    // ── الموافقة على العملاء ──────────────────────────────────────────────────

    /// <summary>جميع العملاء المعلقين</summary>
    [HttpGet("customers/pending")]
    public async Task<IActionResult> GetPendingCustomers()
    {
        var pending = await db.Customers
            .Where(c => !c.IsApproved)
            .Include(c => c.Employee)
            .Select(c => new
            {
                c.Id, c.FullName, c.StoreName, c.Phone, c.Address, c.Region,
                RepName = c.Employee != null ? c.Employee.FullName : null,
                c.CreatedAt
            }).ToListAsync();
        return Ok(ApiResponse<object>.Ok(pending));
    }

    /// <summary>الموافقة على عميل</summary>
    [HttpPost("customers/{id:int}/approve")]
    public async Task<IActionResult> ApproveCustomer(int id)
    {
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return NotFound(ApiResponse<object>.Fail("العميل غير موجود", "Not found"));
        customer.IsApproved = true;
        await db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "تمت الموافقة"));
    }

    // ── الفواتير والموافقة ────────────────────────────────────────────────────

    /// <summary>فواتير معلقة بانتظار موافقة مدير الحسابات</summary>
    [HttpGet("invoices/pending")]
    public async Task<IActionResult> GetPendingInvoices()
    {
        var result = await mediator.Send(new GetAllInvoicesQuery(Status: Domain.Enums.InvoiceStatus.Pending));
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>الموافقة على فاتورة</summary>
    [HttpPost("invoices/{id:int}/approve")]
    public async Task<IActionResult> ApproveInvoice(int id)
    {
        var ok = await mediator.Send(new ApproveInvoiceCommand(id, ManagerId));
        return ok
            ? Ok(ApiResponse<bool>.Ok(true, "تمت الموافقة على الفاتورة"))
            : BadRequest(ApiResponse<bool>.Fail("لا يمكن الموافقة على هذه الفاتورة", "Cannot approve"));
    }

    /// <summary>رفض فاتورة</summary>
    [HttpPost("invoices/{id:int}/reject")]
    public async Task<IActionResult> RejectInvoice(int id)
    {
        var ok = await mediator.Send(new RejectInvoiceCommand(id, ManagerId));
        return ok
            ? Ok(ApiResponse<bool>.Ok(true, "تم رفض الفاتورة"))
            : BadRequest(ApiResponse<bool>.Fail("خطأ في الرفض", "Error"));
    }

    // ── التقارير ──────────────────────────────────────────────────────────────

    /// <summary>ملخص مبيعات شامل</summary>
    [HttpGet("reports/summary")]
    public async Task<IActionResult> GetSalesSummary([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var q = db.Invoices.AsQueryable();
        if (from.HasValue) q = q.Where(i => i.OrderDate >= from.Value);
        if (to.HasValue) q = q.Where(i => i.OrderDate <= to.Value);

        var summary = await q.GroupBy(_ => 1).Select(g => new
        {
            TotalInvoices = g.Count(),
            TotalAmount = g.Sum(i => i.TotalAmount),
            TotalPaid = g.Sum(i => i.PaidAmount),
            TotalDebt = g.Sum(i => i.TotalAmount - i.PaidAmount),
            Pending = g.Count(i => i.Status == Domain.Enums.InvoiceStatus.Pending),
            Completed = g.Count(i => i.Status == Domain.Enums.InvoiceStatus.Completed),
            Rejected = g.Count(i => i.Status == Domain.Enums.InvoiceStatus.Rejected)
        }).FirstOrDefaultAsync();

        return Ok(ApiResponse<object>.Ok(summary));
    }

    /// <summary>ملخص ديون العملاء</summary>
    [HttpGet("reports/debts")]
    public async Task<IActionResult> GetDebtReport()
    {
        var debts = await db.Invoices
            .Where(i => i.TotalAmount > i.PaidAmount)
            .Include(i => i.Customer)
            .GroupBy(i => new { i.CustomerId, i.Customer!.FullName, i.Customer.StoreName })
            .Select(g => new
            {
                g.Key.CustomerId,
                g.Key.FullName,
                g.Key.StoreName,
                TotalDebt = g.Sum(i => i.TotalAmount - i.PaidAmount)
            })
            .OrderByDescending(x => x.TotalDebt)
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(debts));
    }

    /// <summary>تقرير المدفوعات</summary>
    [HttpGet("reports/payments")]
    public async Task<IActionResult> GetPaymentsReport([FromQuery] bool? verified = null)
    {
        var result = await mediator.Send(new GetAllPaymentsQuery(IsVerified: verified));
        return Ok(ApiResponse<object>.Ok(result));
    }
}
