using DeliverySystem.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliverySystem.Domain.Entities;

/// <summary>كيان الفاتورة</summary>
public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? ApprovedByEmployeeId { get; set; }
    public Employee? ApprovedByEmployee { get; set; }

    public int? BranchId { get; set; }
    public Branch? BranchEntity { get; set; }

    public InvoiceSource InvoiceSource { get; set; } = InvoiceSource.Customer;
    public string? PromoCode { get; set; }
    public string? AppliedOfferSummary { get; set; }

    public DeliveryScheduleType DeliveryScheduleType { get; set; } = DeliveryScheduleType.Immediate;
    public DateTime? ScheduledDeliveryDate { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }

    [NotMapped]
    public decimal RemainingAmount => TotalAmount - PaidAmount;

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public DateTime? ApprovedAt { get; set; }

    public List<InvoiceDetail> Details { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
}
