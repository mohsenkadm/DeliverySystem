namespace DeliverySystem.Domain.Entities;

/// <summary>كيان سجل النشاطات لتتبع العمليات</summary>
public class ActivityLog
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}
