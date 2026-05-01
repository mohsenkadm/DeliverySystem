using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات الفاتورة</summary>
public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int? ApprovedByEmployeeId { get; set; }
    public string? ApprovedByEmployeeName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public InvoiceSource InvoiceSource { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusText { get; set; } = string.Empty;
    public string? PromoCode { get; set; }
    public string? AppliedOfferSummary { get; set; }
    public int? BranchId { get; set; }
    public List<InvoiceDetailDto> Details { get; set; } = [];
}

/// <summary>نقل بيانات تفاصيل الفاتورة</summary>
public class InvoiceDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal SubTotal { get; set; }
}

/// <summary>بيانات إنشاء فاتورة جديدة</summary>
public class CreateInvoiceDto
{
    public int CustomerId { get; set; }
    public int? EmployeeId { get; set; }
    public InvoiceSource InvoiceSource { get; set; } = InvoiceSource.Customer;
    public string? PromoCode { get; set; }
    public int? BranchId { get; set; }
    public List<CreateInvoiceDetailDto> Details { get; set; } = [];
}

/// <summary>بيانات تفاصيل الفاتورة الجديدة</summary>
public class CreateInvoiceDetailDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}

/// <summary>بيانات دفع الفاتورة</summary>
public class PayInvoiceDto
{
    public decimal Amount { get; set; }
}

/// <summary>بيانات تغيير حالة الفاتورة</summary>
public class UpdateInvoiceStatusDto
{
    public InvoiceStatus Status { get; set; }
}

/// <summary>بيانات تعيين موظف للفاتورة</summary>
public class AssignDriverDto
{
    public int EmployeeId { get; set; }
}

/// <summary>بيانات موافقة مدير الحسابات على فاتورة</summary>
public class ApproveInvoiceDto
{
    public int ApprovedByEmployeeId { get; set; }
}
