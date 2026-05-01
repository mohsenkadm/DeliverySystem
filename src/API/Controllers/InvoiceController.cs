using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Invoices.Commands;
using DeliverySystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliverySystem.API.Controllers;

/// <summary>Controller إدارة الفواتير لجميع الأدوار</summary>
[ApiController]
[Route("api/invoices")]
[Authorize]
[Produces("application/json")]
public class InvoiceController(IMediator mediator) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>جلب فواتير العميل الحالي</summary>
    [HttpGet("customer")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetCustomerInvoices()
    {
        var result = await mediator.Send(new GetAllInvoicesQuery(CustomerId: CurrentUserId));
        return Ok(ApiResponse<IEnumerable<InvoiceDto>>.Ok(result));
    }

    /// <summary>إنشاء فاتورة جديدة</summary>
    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
    {
        var result = await mediator.Send(new CreateInvoiceCommand(dto));
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "تم إنشاء الفاتورة بنجاح", "Invoice created successfully"));
    }

    /// <summary>جلب فواتير عملاء المندوب الحالي</summary>
    [HttpGet("representative")]
    [Authorize(Roles = "Representative")]
    public async Task<IActionResult> GetRepresentativeInvoices()
    {
        var result = await mediator.Send(new GetAllInvoicesQuery());
        return Ok(ApiResponse<IEnumerable<InvoiceDto>>.Ok(result));
    }

    /// <summary>جلب فواتير السائق الحالي (بدون مبالغ)</summary>
    [HttpGet("driver")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> GetDriverInvoices()
    {
        var result = await mediator.Send(new GetAllInvoicesQuery(DriverId: CurrentUserId));
        var simplified = result.Select(i => new
        {
            i.Id, i.InvoiceNumber, i.OrderDate, i.CustomerId, i.CustomerName, i.Status, i.StatusText
        });
        return Ok(ApiResponse<object>.Ok(simplified));
    }

    /// <summary>دفع مبلغ للفاتورة (كامل أو جزئي) بواسطة المندوب</summary>
    [HttpPost("{id:int}/pay")]
    [Authorize(Roles = "Representative,Admin")]
    public async Task<IActionResult> Pay(int id, [FromBody] PayInvoiceDto dto)
    {
        var result = await mediator.Send(new PayInvoiceCommand(id, dto.Amount));
        if (!result) return NotFound(ApiResponse<bool>.Fail("الفاتورة غير موجودة", "Invoice not found"));
        return Ok(ApiResponse<bool>.Ok(true, "تم تسجيل الدفع بنجاح", "Payment recorded successfully"));
    }

    /// <summary>تغيير حالة الفاتورة بواسطة السائق</summary>
    [HttpPatch("{id:int}/status/driver")]
    [Authorize(Roles = "Driver")]
    public async Task<IActionResult> UpdateStatusDriver(int id, [FromBody] UpdateInvoiceStatusDto dto)
    {
        var allowed = new[] { InvoiceStatus.Completed, InvoiceStatus.AwaitingDelivery };
        if (!allowed.Contains(dto.Status))
            return BadRequest(ApiResponse<bool>.Fail("الحالة غير مسموح بها للسائق", "Status not allowed for driver"));
        var result = await mediator.Send(new UpdateInvoiceStatusCommand(id, dto.Status));
        if (!result) return NotFound(ApiResponse<bool>.Fail("الفاتورة غير موجودة", "Invoice not found"));
        return Ok(ApiResponse<bool>.Ok(true, "تم تحديث الحالة", "Status updated"));
    }

    /// <summary>تغيير حالة الفاتورة بواسطة الأدمن</summary>
    [HttpPatch("{id:int}/status/admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatusAdmin(int id, [FromBody] UpdateInvoiceStatusDto dto)
    {
        var result = await mediator.Send(new UpdateInvoiceStatusCommand(id, dto.Status));
        if (!result) return NotFound(ApiResponse<bool>.Fail("الفاتورة غير موجودة", "Invoice not found"));
        return Ok(ApiResponse<bool>.Ok(true, "تم تحديث الحالة", "Status updated"));
    }
}
