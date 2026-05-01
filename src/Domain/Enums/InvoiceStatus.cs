namespace DeliverySystem.Domain.Enums;

/// <summary>حالات الفاتورة</summary>
public enum InvoiceStatus
{
    Pending = 0,              // معلق – بانتظار موافقة مدير الحسابات
    Deferred = 1,             // مؤجل
    AwaitingDelivery = 2,     // في انتظار التوصيل – السائق في الطريق
    Completed = 3,            // مكتمل – تم التسليم والدفع
    Rejected = 4,             // مرفوض
    Accepted = 5,             // مقبول – وافق مدير الحسابات
    WarehouseProcessing = 6,  // يعالجه المستودع – جاري التجهيز
    Delivered = 7             // تم التوصيل – بانتظار تسوية الدفع
}
