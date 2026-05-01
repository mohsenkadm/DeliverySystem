using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات الإشعار</summary>
public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationTarget Target { get; set; }
    public int? TargetUserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
