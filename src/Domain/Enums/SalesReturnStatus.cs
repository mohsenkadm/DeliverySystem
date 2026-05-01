namespace DeliverySystem.Domain.Enums;

public enum SalesReturnStatus
{
    Pending            = 0,  // بانتظار موافقة المدير
    ManagerApproved    = 1,  // وافق عليه المدير — بانتظار المستودع
    WarehouseApproved  = 2,  // وافق المستودع — بانتظار المحاسب
    AccountantApproved = 3,  // وافق المحاسب — جاهز للتنفيذ
    Completed          = 4,  // مكتملة
    Rejected           = 5   // مرفوضة
}
