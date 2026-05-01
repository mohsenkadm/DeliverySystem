using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Invoices.Commands;
using DeliverySystem.Application.Features.Payments.Commands;
using DeliverySystem.Application.Features.TransferOrders.Commands;
using DeliverySystem.Domain.Enums;
using DeliverySystem.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DeliverySystem.API.Controllers;

/// <summary>API المندوب في تطبيق الجوال</summary>
[ApiController]
[Route("api/mobile/rep")]
[Authorize(Roles = "Representative,Employee")]
[Produces("application/json")]
public class MobileRepresentativeController(IMediator mediator, ApplicationDbContext db) : ControllerBase
{
    private int RepId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // ── العملاء ──────────────────────────────────────────────────────────────

    /// <summary>عملاء المندوب</summary>
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] bool? pendingApproval = null)
    {
        var q = db.Customers.Where(c => c.EmployeeId == RepId);
        if (pendingApproval.HasValue) q = q.Where(c => c.IsApproved == !pendingApproval.Value);
        var list = await q.Select(c => new
        {
            c.Id, c.FullName, c.StoreName, c.Phone, c.Address,
            c.Region, c.ClientType, c.IsApproved,
            Balance = db.Invoices
                .Where(i => i.CustomerId == c.Id)
                .Sum(i => i.TotalAmount - i.PaidAmount)
        }).ToListAsync();
        return Ok(ApiResponse<object>.Ok(list));
    }

    /// <summary>إضافة عميل جديد (يحتاج موافقة المشرف)</summary>
    [HttpPost("customers")]
    public async Task<IActionResult> AddCustomer([FromBody] CreateCustomerByRepDto dto)
    {
        var customer = new Domain.Entities.Customer
        {
            FullName = dto.FullName,
            StoreName = dto.StoreName,
            Phone = dto.Phone,
            Address = dto.Address,
            Region = dto.Region,
            ClientType = dto.ClientType ?? "Wholesale",
            Username = dto.Phone,
            PasswordHash = string.Empty,
            IsApproved = false,
            EmployeeId = RepId
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { customer.Id, customer.FullName, customer.IsApproved }, "تم إضافة العميل بانتظار الموافقة"));
    }

    // ── الفواتير ──────────────────────────────────────────────────────────────

    /// <summary>فواتير صادرة من هذا المندوب</summary>
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices([FromQuery] InvoiceStatus? status = null)
    {
        var result = await mediator.Send(new GetAllInvoicesQuery(EmployeeId: RepId, Status: status));
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>إصدار فاتورة لعميل</summary>
    [HttpPost("invoices")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
    {
        dto.EmployeeId = RepId;
        dto.InvoiceSource = InvoiceSource.Representative;
        var result = await mediator.Send(new CreateInvoiceCommand(dto));
        return Ok(ApiResponse<InvoiceDto>.Ok(result));
    }

    /// <summary>تفاصيل فاتورة</summary>
    [HttpGet("invoices/{id:int}")]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var result = await mediator.Send(new GetInvoiceByIdQuery(id));
        if (result is null) return NotFound(ApiResponse<object>.Fail("الفاتورة غير موجودة", "Not found"));
        return Ok(ApiResponse<InvoiceDto>.Ok(result));
    }

    // ── الدفعات والتحصيل ──────────────────────────────────────────────────────

    /// <summary>تسجيل دفعة محصلة من العميل</summary>
    [HttpPost("payments/collect")]
    public async Task<IActionResult> CollectPayment([FromBody] CollectPaymentDto dto)
    {
        var createDto = new CreatePaymentDto
        {
            InvoiceId = dto.InvoiceId,
            CustomerId = dto.CustomerId,
            PaidByEmployeeId = RepId,
            Amount = dto.Amount,
            Type = PaymentType.CustomerToRepresentative,
            Notes = dto.Notes
        };
        var result = await mediator.Send(new RecordPaymentCommand(createDto));
        return Ok(ApiResponse<PaymentDto>.Ok(result, "تم تسجيل التحصيل"));
    }

    /// <summary>تسليم المبلغ للشركة (كاشير)</summary>
    [HttpPost("payments/submit")]
    public async Task<IActionResult> SubmitPayment([FromBody] SubmitPaymentDto dto)
    {
        var createDto = new CreatePaymentDto
        {
            InvoiceId = dto.InvoiceId,
            PaidByEmployeeId = RepId,
            Amount = dto.Amount,
            Type = PaymentType.RepresentativeToCompany,
            Notes = dto.Notes
        };
        var result = await mediator.Send(new RecordPaymentCommand(createDto));
        return Ok(ApiResponse<PaymentDto>.Ok(result, "تم تسجيل التسليم للشركة"));
    }

    /// <summary>سجل الدفعات الخاصة بالمندوب</summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments()
    {
        var result = await mediator.Send(new GetAllPaymentsQuery(EmployeeId: RepId));
        return Ok(ApiResponse<object>.Ok(result));
    }

    // ── الديون ───────────────────────────────────────────────────────────────

    /// <summary>ديون عملاء المندوب</summary>
    [HttpGet("debts")]
    public async Task<IActionResult> GetDebts()
    {
        var debts = await db.Invoices
            .Where(i => i.EmployeeId == RepId && i.TotalAmount > i.PaidAmount)
            .Include(i => i.Customer)
            .GroupBy(i => new { i.CustomerId, i.Customer!.FullName, i.Customer.StoreName, i.Customer.Phone })
            .Select(g => new
            {
                g.Key.CustomerId,
                g.Key.FullName,
                g.Key.StoreName,
                g.Key.Phone,
                TotalDebt = g.Sum(i => i.TotalAmount - i.PaidAmount),
                InvoiceCount = g.Count()
            }).ToListAsync();
        return Ok(ApiResponse<object>.Ok(debts));
    }

    // ── مستودع المندوب ────────────────────────────────────────────────────────

    /// <summary>مخزون مستودع المندوب</summary>
    [HttpGet("warehouse")]
    public async Task<IActionResult> GetWarehouseInventory()
    {
        var warehouse = await db.Warehouses
            .Include(w => w.Inventories).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(w => w.OwnerEmployeeId == RepId && w.IsSubWarehouse);

        if (warehouse is null)
            return NotFound(ApiResponse<object>.Fail("لا يوجد مستودع مخصص لك", "No sub-warehouse assigned"));

        var inventory = warehouse.Inventories.Select(i => new
        {
            i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name,
            i.Quantity,
            WarehouseId = warehouse.Id,
            WarehouseName = warehouse.Name
        });
        return Ok(ApiResponse<object>.Ok(inventory));
    }

    // ── طلبات النقل ──────────────────────────────────────────────────────────

    /// <summary>طلب نقل بضاعة من المستودع الرئيسي لمستودع المندوب</summary>
    [HttpPost("transfer-orders")]
    public async Task<IActionResult> RequestTransfer([FromBody] CreateTransferOrderDto dto)
    {
        dto.OrderType = TransferOrderType.OutboundToRepWarehouse;
        var result = await mediator.Send(new CreateTransferOrderCommand(dto, RepId));
        return Ok(ApiResponse<TransferOrderDto>.Ok(result));
    }

    /// <summary>طلب إرجاع بضاعة من مستودع المندوب للمستودع الرئيسي</summary>
    [HttpPost("transfer-orders/return")]
    public async Task<IActionResult> RequestReturn([FromBody] CreateTransferOrderDto dto)
    {
        dto.OrderType = TransferOrderType.ReturnToMainWarehouse;
        var result = await mediator.Send(new CreateTransferOrderCommand(dto, RepId));
        return Ok(ApiResponse<TransferOrderDto>.Ok(result));
    }

    /// <summary>طلبات النقل الخاصة بالمندوب</summary>
    [HttpGet("transfer-orders")]
    public async Task<IActionResult> GetTransferOrders([FromQuery] TransferOrderStatus? status = null)
    {
        var result = await mediator.Send(new GetAllTransferOrdersQuery(RequestedByEmployeeId: RepId, Status: status));
        return Ok(ApiResponse<object>.Ok(result));
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

public class CreateCustomerByRepDto
{
    public string FullName { get; set; } = string.Empty;
    public string? StoreName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Region { get; set; }
    public string? ClientType { get; set; }
}

public class CollectPaymentDto
{
    public int? InvoiceId { get; set; }
    public int? CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class SubmitPaymentDto
{
    public int? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
