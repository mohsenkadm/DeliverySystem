namespace DeliverySystem.Domain.Enums;

/// <summary>حالات الدفع للفاتورة</summary>
public enum PaymentStatus
{
    Unpaid = 0,       // غير مدفوع
    PartialPaid = 1,  // مدفوع جزئياً
    FullPaid = 2      // مدفوع بالكامل
}
