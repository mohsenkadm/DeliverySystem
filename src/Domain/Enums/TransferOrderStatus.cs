namespace DeliverySystem.Domain.Enums;

public enum TransferOrderStatus
{
    Pending = 0,            // في انتظار الموافقة
    AccountantApproved = 1, // موافقة المحاسب
    WarehouseProcessing = 2,// يعالجه المستودع
    Completed = 3,          // مكتمل
    Rejected = 4,           // مرفوض
    ReturnPending = 5,      // طلب إرجاع معلق
    ReturnApproved = 6,     // تمت الموافقة على الإرجاع
    ReturnCompleted = 7     // اكتمل الإرجاع
}
