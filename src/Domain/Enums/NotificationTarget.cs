namespace DeliverySystem.Domain.Enums;

/// <summary>المستهدفون بالإشعار</summary>
public enum NotificationTarget
{
    Admin = 0,          // المسؤول
    Driver = 1,         // السائق
    Representative = 2, // المندوب
    Customer = 3,       // العميل
    Employee = 4,       // موظف عام
    Supervisor = 5,     // المشرف
    SalesManager = 6    // مدير المبيعات
}
