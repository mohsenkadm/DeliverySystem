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

/// <summary>API المشرف على المندوبين</summary>
[ApiController]
[Route("api/mobile/supervisor")]
[Authorize(Roles = "Supervisor,Employee")]
[Produces("application/json")]
public class MobileSupervisorController(IMediator mediator, ApplicationDbContext db) : ControllerBase
{
    private int SupervisorId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // ── المندوبون ─────────────────────────────────────────────────────────────

    /// <summary>المندوبون التابعون لهذا المشرف</summary>
    [HttpGet("reps")]
    public async Task<IActionResult> GetReps()
    {
        var reps = await db.Employees
            .Where(e => e.SupervisorId == SupervisorId && e.IsActive)
            .Select(e => new
            {
                e.Id, e.FullName, e.Phone, e.EmployeeType, e.Roles,
                CustomerCount = db.Customers.Count(c => c.EmployeeId == e.Id),
                TotalInvoices = db.Invoices.Count(i => i.EmployeeId == e.Id),
                TotalCollected = db.Payments
                    .Where(p => p.PaidByEmployeeId == e.Id && p.IsVerified)
                    .Sum(p => p.Amount)
            }).ToListAsync();
        return Ok(ApiResponse<object>.Ok(reps));
    }

    /// <summary>فواتير مندوب محدد</summary>
    [HttpGet("reps/{repId:int}/invoices")]
    public async Task<IActionResult> GetRepInvoices(int repId, [FromQuery] Domain.Enums.InvoiceStatus? status = null)
    {
        if (!await IsMyRep(repId)) return Forbid();
        var result = await mediator.Send(new GetAllInvoicesQuery(EmployeeId: repId, Status: status));
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>دفعات مندوب محدد</summary>
    [HttpGet("reps/{repId:int}/payments")]
    public async Task<IActionResult> GetRepPayments(int repId)
    {
        if (!await IsMyRep(repId)) return Forbid();
        var result = await mediator.Send(new GetAllPaymentsQuery(EmployeeId: repId));
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>عملاء مندوب محدد</summary>
    [HttpGet("reps/{repId:int}/customers")]
    public async Task<IActionResult> GetRepCustomers(int repId)
    {
        if (!await IsMyRep(repId)) return Forbid();
        var customers = await db.Customers
            .Where(c => c.EmployeeId == repId)
            .Select(c => new
            {
                c.Id, c.FullName, c.StoreName, c.Phone, c.Address, c.IsApproved,
                Debt = db.Invoices.Where(i => i.CustomerId == c.Id).Sum(i => i.TotalAmount - i.PaidAmount)
            }).ToListAsync();
        return Ok(ApiResponse<object>.Ok(customers));
    }

    // ── الموافقة على العملاء ──────────────────────────────────────────────────

    /// <summary>عملاء معلقون بانتظار الموافقة</summary>
    [HttpGet("customers/pending")]
    public async Task<IActionResult> GetPendingCustomers()
    {
        var myRepIds = await db.Employees
            .Where(e => e.SupervisorId == SupervisorId)
            .Select(e => e.Id).ToListAsync();

        var pending = await db.Customers
            .Where(c => !c.IsApproved && c.EmployeeId != null && myRepIds.Contains(c.EmployeeId!.Value))
            .Include(c => c.Employee)
            .Select(c => new
            {
                c.Id, c.FullName, c.StoreName, c.Phone, c.Address,
                RepName = c.Employee!.FullName, c.CreatedAt
            }).ToListAsync();
        return Ok(ApiResponse<object>.Ok(pending));
    }

    /// <summary>الموافقة على عميل</summary>
    [HttpPost("customers/{id:int}/approve")]
    public async Task<IActionResult> ApproveCustomer(int id)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id && !c.IsApproved);
        if (customer is null) return NotFound(ApiResponse<object>.Fail("العميل غير موجود", "Not found"));
        customer.IsApproved = true;
        await db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "تمت الموافقة على العميل"));
    }

    /// <summary>رفض طلب عميل</summary>
    [HttpPost("customers/{id:int}/reject")]
    public async Task<IActionResult> RejectCustomer(int id)
    {
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id && !c.IsApproved);
        if (customer is null) return NotFound(ApiResponse<object>.Fail("العميل غير موجود", "Not found"));
        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "تم رفض طلب العميل"));
    }

    // ── التقارير ──────────────────────────────────────────────────────────────

    /// <summary>ملخص مبيعات المندوبين التابعين</summary>
    [HttpGet("reports/sales")]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var myRepIds = await db.Employees
            .Where(e => e.SupervisorId == SupervisorId)
            .Select(e => e.Id).ToListAsync();

        var q = db.Invoices.Where(i => i.EmployeeId != null && myRepIds.Contains(i.EmployeeId!.Value));
        if (from.HasValue) q = q.Where(i => i.OrderDate >= from.Value);
        if (to.HasValue) q = q.Where(i => i.OrderDate <= to.Value);

        var summary = await q.GroupBy(i => new { i.EmployeeId })
            .Select(g => new
            {
                RepId = g.Key.EmployeeId,
                RepName = db.Employees.Where(e => e.Id == g.Key.EmployeeId).Select(e => e.FullName).FirstOrDefault(),
                TotalInvoices = g.Count(),
                TotalAmount = g.Sum(i => i.TotalAmount),
                TotalPaid = g.Sum(i => i.PaidAmount),
                TotalDebt = g.Sum(i => i.TotalAmount - i.PaidAmount)
            }).ToListAsync();

        return Ok(ApiResponse<object>.Ok(summary));
    }

    private async Task<bool> IsMyRep(int repId)
        => await db.Employees.AnyAsync(e => e.Id == repId && e.SupervisorId == SupervisorId);
}
