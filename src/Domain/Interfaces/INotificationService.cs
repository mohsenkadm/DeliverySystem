using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Domain.Interfaces;

/// <summary>واجهة خدمة الإشعارات (OneSignal + DB)</summary>
public interface INotificationService
{
    /// <summary>إرسال إشعار وحفظه في قاعدة البيانات</summary>
    Task SendNotificationAsync(string title, string body, NotificationTarget target, int? targetUserId = null);

    /// <summary>إرسال إشعار OneSignal عبر REST API</summary>
    Task SendOneSignalNotificationAsync(string title, string body, string? playerId = null);
}
