using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Invoices.Commands;
using DeliverySystem.Application.Features.Products.Commands;
using DeliverySystem.Domain.Enums;
using DeliverySystem.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace DeliverySystem.API.Controllers;

/// <summary>API العميل في تطبيق الجوال</summary>
[ApiController]
[Route("api/mobile/customer")]
[Authorize(Roles = "Customer")]
[Produces("application/json")]
public class MobileCustomerController(IMediator mediator, ApplicationDbContext db) : ControllerBase
{
    private int CurrentCustomerId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // ── المنتجات ────────────────────────────────────────────────────────────────

    /// <summary>تصفح المنتجات (مع فلتر الفرع والتصنيف)</summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string?  search     = null,
        [FromQuery] int?     categoryId = null,
        [FromQuery] int?     branchId   = null,
        [FromQuery] int      page       = 1,
        [FromQuery] int      pageSize   = 20)
    {
        var q = db.Products.Include(p => p.Category)
            .Include(p => p.Inventories).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
        if (categoryId.HasValue)
            q = q.Where(p => p.CategoryId == categoryId.Value);

        if (branchId.HasValue)
        {
            // Filter by branch via warehouse inventories linked to branch
            var warehouseIds = await db.Warehouses
                .Where(w => w.BranchId == branchId)
                .Select(w => w.Id)
                .ToListAsync();
            q = q.Where(p => p.Inventories.Any(i => warehouseIds.Contains(i.WarehouseId) && i.Quantity > 0));
        }

        var total = await q.CountAsync();
        var products = await q
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id, p.Name, p.Code, p.Description,
                p.WholesalePrice, p.RetailPrice, p.DiscountPercentage,
                p.CartonType, p.BaseQuantity, p.ImagePath,
                CategoryName   = p.Category != null ? p.Category.Name : null,
                TotalStock     = p.Inventories.Sum(i => i.Quantity),
                IsInStock      = p.Inventories.Sum(i => i.Quantity) > 0
            }).ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { total, page, pageSize, data = products }));
    }

    // ── الطلبات ─────────────────────────────────────────────────────────────────

    /// <summary>قائمة طلبات العميل</summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] InvoiceStatus? status = null)
    {
        var result = await mediator.Send(new GetAllInvoicesQuery(
            CustomerId: CurrentCustomerId,
            Status:     status));
        return Ok(ApiResponse<IEnumerable<InvoiceDto>>.Ok(result));
    }

    /// <summary>تفاصيل طلب واحد</summary>
    [HttpGet("orders/{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id));
        if (invoice is null || invoice.CustomerId != CurrentCustomerId)
            return NotFound(ApiResponse<object>.Fail("الطلب غير موجود", "Order not found"));
        return Ok(ApiResponse<InvoiceDto>.Ok(invoice));
    }

    /// <summary>إنشاء طلب جديد</summary>
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateInvoiceDto dto)
    {
        dto.CustomerId = CurrentCustomerId;
        var result = await mediator.Send(new CreateInvoiceCommand(dto));
        return Ok(ApiResponse<InvoiceDto>.Ok(result, "تم إنشاء الطلب بنجاح", "Order created successfully"));
    }

    /// <summary>إلغاء طلب (فقط في حالة معلق)</summary>
    [HttpPost("orders/{id:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id));
        if (invoice is null || invoice.CustomerId != CurrentCustomerId)
            return NotFound(ApiResponse<object>.Fail("الطلب غير موجود", "Order not found"));
        if (invoice.Status != InvoiceStatus.Pending)
            return BadRequest(ApiResponse<object>.Fail("لا يمكن إلغاء هذا الطلب", "Cannot cancel this order"));
        await mediator.Send(new UpdateInvoiceStatusCommand(id, InvoiceStatus.Rejected));
        return Ok(ApiResponse<bool>.Ok(true, "تم إلغاء الطلب", "Order cancelled"));
    }

    // ── الفاتورة PDF ─────────────────────────────────────────────────────────────

    /// <summary>عرض الفاتورة بتنسيق HTML قابل للطباعة (PDF)</summary>
    [HttpGet("orders/{id:int}/invoice")]
    public async Task<IActionResult> GetInvoicePdf(int id)
    {
        var invoice = await mediator.Send(new GetInvoiceByIdQuery(id));
        if (invoice is null || invoice.CustomerId != CurrentCustomerId)
            return NotFound(ApiResponse<object>.Fail("الفاتورة غير موجودة", "Invoice not found"));

        var html = BuildInvoiceHtml(invoice);
        return Content(html, "text/html", Encoding.UTF8);
    }

    private static string BuildInvoiceHtml(InvoiceDto inv)
    {
        var rows = new StringBuilder();
        foreach (var d in inv.Details)
            rows.AppendLine($"<tr><td>{d.ProductName}</td><td>{d.Quantity}</td><td>{d.UnitPrice:N2}</td><td>{d.Discount:N2}</td><td>{d.SubTotal:N2}</td></tr>");

        var offerBanner = string.IsNullOrEmpty(inv.AppliedOfferSummary)
            ? ""
            : $"<div class=\"offer\">عروض مطبقة: {inv.AppliedOfferSummary}</div>";

        var html = new StringBuilder();
        html.Append("<!DOCTYPE html><html lang=\"ar\" dir=\"rtl\"><head><meta charset=\"utf-8\"/>");
        html.Append($"<title>فاتورة {inv.InvoiceNumber}</title>");
        html.Append("<style>");
        html.Append("body{font-family:Arial,sans-serif;margin:20px;direction:rtl}");
        html.Append("table{width:100%;border-collapse:collapse;margin-top:15px}");
        html.Append("th,td{border:1px solid #ddd;padding:8px;text-align:right}");
        html.Append("th{background:#4f46e5;color:#fff}");
        html.Append(".header{display:flex;justify-content:space-between;margin-bottom:20px}");
        html.Append(".total{font-size:1.3em;font-weight:bold;text-align:left;margin-top:15px}");
        html.Append(".offer{background:#fef3c7;padding:8px;border-radius:6px;margin-top:10px}");
        html.Append("@media print{button{display:none}}");
        html.Append("</style></head><body>");
        html.Append("<div class=\"header\">");
        html.Append("<div><h2>نظام إدارة المبيعات والتوصيل</h2></div>");
        html.Append($"<div><h3>فاتورة: {inv.InvoiceNumber}</h3><p>{inv.OrderDate:yyyy/MM/dd}</p></div>");
        html.Append("</div>");
        html.Append($"<p><strong>العميل:</strong> {inv.CustomerName}</p>");
        html.Append($"<p><strong>الموظف:</strong> {inv.EmployeeName ?? "—"}</p>");
        html.Append($"<p><strong>الحالة:</strong> {inv.StatusText}</p>");
        html.Append(offerBanner);
        html.Append("<table><thead><tr><th>المنتج</th><th>الكمية</th><th>السعر</th><th>الخصم</th><th>الإجمالي</th></tr></thead>");
        html.Append($"<tbody>{rows}</tbody></table>");
        html.Append("<div class=\"total\">");
        html.Append($"<p>الإجمالي: {inv.TotalAmount:N2} ر.س</p>");
        html.Append($"<p>المدفوع: {inv.PaidAmount:N2} ر.س</p>");
        html.Append($"<p>المتبقي: {inv.RemainingAmount:N2} ر.س</p>");
        html.Append("</div>");
        html.Append("<button onclick=\"window.print()\">طباعة</button>");
        html.Append("</body></html>");
        return html.ToString();
    }

    // ── الديون والمدفوعات ─────────────────────────────────────────────────────────

    /// <summary>ملخص ديون العميل</summary>
    [HttpGet("debts")]
    public async Task<IActionResult> GetDebts()
    {
        var invoices = await mediator.Send(new GetAllInvoicesQuery(CustomerId: CurrentCustomerId));
        var all      = invoices.ToList();
        var debts    = all.Where(i => i.RemainingAmount > 0).ToList();
        return Ok(ApiResponse<object>.Ok(new
        {
            TotalDebt        = debts.Sum(i => i.RemainingAmount),
            TotalPaid        = all.Sum(i => i.PaidAmount),
            TotalInvoices    = all.Count,
            UnpaidInvoices   = debts.Count,
            PendingInvoices  = all.Count(i => i.Status == InvoiceStatus.Pending),
            Invoices         = debts.Select(i => new { i.Id, i.InvoiceNumber, i.OrderDate, i.TotalAmount, i.PaidAmount, i.RemainingAmount, i.StatusText })
        }));
    }

    // ── الإشعارات ────────────────────────────────────────────────────────────────

    /// <summary>إشعارات العميل</summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var customerId = CurrentCustomerId;
        var notifs = await db.Notifications
            .Where(n => n.Target == NotificationTarget.Customer &&
                        (n.TargetUserId == null || n.TargetUserId == customerId))
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new { n.Id, n.Title, n.Body, n.IsRead, n.CreatedAt })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(notifs));
    }

    /// <summary>تحديد إشعار كمقروء</summary>
    [HttpPatch("notifications/{id:int}/read")]
    public async Task<IActionResult> MarkNotificationRead(int id)
    {
        var notif = await db.Notifications.FindAsync(id);
        if (notif is null) return NotFound();
        notif.IsRead = true;
        await db.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
