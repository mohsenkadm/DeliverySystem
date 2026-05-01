using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Domain.Entities;

/// <summary>سجل دفعة مالية (عميل → سائق/مندوب → شركة)</summary>
public class Payment
{
    public int Id { get; set; }

    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>من دفع (السائق أو المندوب الذي جمع المبلغ)</summary>
    public int? PaidByEmployeeId { get; set; }
    public Employee? PaidByEmployee { get; set; }

    /// <summary>من استلم (الكاشير أو المشرف)</summary>
    public int? ReceivedByEmployeeId { get; set; }
    public Employee? ReceivedByEmployee { get; set; }

    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public string? Notes { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}
