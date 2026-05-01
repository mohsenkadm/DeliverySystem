using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Domain.Entities;

/// <summary>طلب مرتجع مبيعات</summary>
public class SalesReturn
{
    public int Id { get; set; }

    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public int RequestedByEmployeeId { get; set; }
    public Employee? RequestedByEmployee { get; set; }

    public int? ApprovedByManagerId { get; set; }
    public Employee? ApprovedByManager { get; set; }

    public SalesReturnStatus Status { get; set; } = SalesReturnStatus.Pending;

    public string Reason { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public string? Notes { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<SalesReturnDetail> Details { get; set; } = [];
}
