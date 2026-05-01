using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Invoices.Commands;
using DeliverySystem.Application.Features.Payments.Commands;
using DeliverySystem.Domain.Enums;
using DeliverySystem.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DeliverySystem.API.Controllers;

/// <summary>API السائق / الموظف في تطبيق الجوال</summary>
[ApiController]
[Route("api/mobile/driver")]
[Authorize(Roles = "Driver,Employee")]
[Produces("application/json")]
public class MobileDriverController(IMediator mediator, ApplicationDbContext db) : ControllerBase
{
    private int CurrentDriverId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // ── الطلبات المعينة ─────────────────────────────────────────────────────────

    /// <summary>قائمة الطلبات المعينة للسائق</summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetAssignedOrders([FromQuery] InvoiceStatus? status = null)
    {
        var result = await mediator.Send(new GetAllInvoicesQuery(
            EmployeeId: CurrentDriverId,
            Status:     status));

        // Return simplified view without financial data
        var simplified = result.Select(i => new
        {
            i.Id, i.InvoiceNumber, i.OrderDate,
            i.CustomerId, i.CustomerName,
            i.Status, i.StatusText,
            ItemCount = i.Details.Count
        });
        return Ok(ApiResponse<object>.Ok(simplified));
    }

    /// <summary>تفاصيل طلب مع بيانات العميل والموقع</summary>
    [HttpGet("orders/{id:int}")]
    public async Task<IActionResult> GetOrderDetail(int id)
    {
        var invoice = await db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(i => i.Id == id && i.EmployeeId == CurrentDriverId);

        if (invoice is null)
            return NotFound(ApiResponse<object>.Fail("الطلب غير موجود أو غير مخصص لك", "Order not found or not assigned to you"));

        var result = new
        {
            invoice.Id, invoice.InvoiceNumber, invoice.OrderDate,
            invoice.Status, StatusText = StatusText(invoice.Status),
            Customer = new
            {
                invoice.Customer!.Id,
                invoice.Customer.FullName,
                invoice.Customer.StoreName,
                invoice.Customer.Phone,
                invoice.Customer.Address,
                invoice.Customer.Region,
                invoice.Customer.Latitude,
                invoice.Customer.Longitude,
                GoogleMapsUrl = invoice.Customer.Latitude.HasValue && invoice.Customer.Longitude.HasValue
                    ? $"https://maps.google.com/?q={invoice.Customer.Latitude},{invoice.Customer.Longitude}"
                    : null
            },
            Items = invoice.Details.Select(d => new
            {
                d.ProductId,
                d.Product!.Name,
                d.Quantity,
                d.Product.CartonType,
                d.Product.BaseQuantity
            })
        };
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>تأكيد تسليم الطلب (يغير الحالة إلى Delivered)</summary>
    [HttpPost("orders/{id:int}/deliver")]
    public async Task<IActionResult> ConfirmDelivery(int id)
    {
        var invoice = await db.Invoices
            .FirstOrDefaultAsync(i => i.Id == id && i.EmployeeId == CurrentDriverId);
        if (invoice is null)
            return NotFound(ApiResponse<object>.Fail("الطلب غير موجود", "Order not found"));
        if (invoice.Status != InvoiceStatus.AwaitingDelivery)
            return BadRequest(ApiResponse<object>.Fail(
                "لا يمكن تأكيد التسليم في هذه الحالة",
                "Cannot confirm delivery for this order status"));

        var ok = await mediator.Send(new ConfirmInvoiceDeliveredCommand(id));
        return ok
            ? Ok(ApiResponse<bool>.Ok(true, "تم تأكيد التسليم بنجاح", "Delivery confirmed"))
            : StatusCode(500, ApiResponse<bool>.Fail("حدث خطأ", "Error"));
    }

    /// <summary>تسجيل دفعة محصلة من العميل</summary>
    [HttpPost("orders/{id:int}/collect-payment")]
    public async Task<IActionResult> CollectPayment(int id, [FromBody] DriverCollectPaymentDto dto)
    {
        var invoice = await db.Invoices
            .FirstOrDefaultAsync(i => i.Id == id && i.EmployeeId == CurrentDriverId);
        if (invoice is null)
            return NotFound(ApiResponse<object>.Fail("الطلب غير موجود", "Order not found"));

        var createDto = new CreatePaymentDto
        {
            InvoiceId = id,
            CustomerId = invoice.CustomerId,
            PaidByEmployeeId = CurrentDriverId,
            Amount = dto.Amount,
            Type = PaymentType.CustomerToDriver,
            Notes = dto.Notes
        };
        var result = await mediator.Send(new RecordPaymentCommand(createDto));
        return Ok(ApiResponse<PaymentDto>.Ok(result, "تم تسجيل التحصيل"));
    }

    /// <summary>تسليم المبلغ للشركة (كاشير)</summary>
    [HttpPost("payments/submit")]
    public async Task<IActionResult> SubmitCashToCompany([FromBody] DriverSubmitPaymentDto dto)
    {
        var createDto = new CreatePaymentDto
        {
            InvoiceId = dto.InvoiceId,
            PaidByEmployeeId = CurrentDriverId,
            Amount = dto.Amount,
            Type = PaymentType.DriverToCompany,
            Notes = dto.Notes
        };
        var result = await mediator.Send(new RecordPaymentCommand(createDto));
        return Ok(ApiResponse<PaymentDto>.Ok(result, "تم تسجيل التسليم للشركة"));
    }

    /// <summary>تحديث حالة الطلب (سائق)</summary>
    [HttpPatch("orders/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusDto dto)
    {
        var allowed = new[] { InvoiceStatus.AwaitingDelivery, InvoiceStatus.Completed };
        if (!allowed.Contains(dto.Status))
            return BadRequest(ApiResponse<object>.Fail(
                "الحالة غير مسموح بها للسائق",
                "Status not allowed for driver"));

        var invoice = await db.Invoices
            .FirstOrDefaultAsync(i => i.Id == id && i.EmployeeId == CurrentDriverId);
        if (invoice is null)
            return NotFound(ApiResponse<object>.Fail("الطلب غير موجود", "Order not found"));

        var ok = await mediator.Send(new UpdateInvoiceStatusCommand(id, dto.Status));
        return ok
            ? Ok(ApiResponse<bool>.Ok(true, "تم تحديث الحالة", "Status updated"))
            : StatusCode(500, ApiResponse<bool>.Fail("حدث خطأ", "Error"));
    }

    /// <summary>ملخص أداء السائق</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var driverId = CurrentDriverId;
        var invoices = await db.Invoices
            .Where(i => i.EmployeeId == driverId)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            TotalAssigned          = invoices.Count,
            Completed              = invoices.Count(i => i.Status == InvoiceStatus.Completed),
            AwaitingDelivery       = invoices.Count(i => i.Status == InvoiceStatus.AwaitingDelivery),
            Rejected               = invoices.Count(i => i.Status == InvoiceStatus.Rejected),
            CompletionRate         = invoices.Count == 0 ? 0
                : Math.Round((double)invoices.Count(i => i.Status == InvoiceStatus.Completed) / invoices.Count * 100, 1)
        }));
    }

    private static string StatusText(InvoiceStatus s) => s switch
    {
        InvoiceStatus.Pending             => "معلق",
        InvoiceStatus.Accepted            => "مقبول",
        InvoiceStatus.WarehouseProcessing => "جاري التجهيز",
        InvoiceStatus.AwaitingDelivery    => "في التوصيل",
        InvoiceStatus.Delivered           => "تم التسليم",
        InvoiceStatus.Completed           => "مكتمل",
        InvoiceStatus.Rejected            => "مرفوض",
        InvoiceStatus.Deferred            => "مؤجل",
        _                                 => ""
    };
}

public class DriverCollectPaymentDto
{
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public class DriverSubmitPaymentDto
{
    public int? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
