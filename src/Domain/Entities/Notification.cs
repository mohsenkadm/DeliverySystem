using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Domain.Entities;

/// <summary>كيان الإشعار</summary>
public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationTarget Target { get; set; }

    /// <summary>معرف المستخدم المستهدف بالإشعار (null = لجميع المستخدمين من نفس النوع)</summary>
    public int? TargetUserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
