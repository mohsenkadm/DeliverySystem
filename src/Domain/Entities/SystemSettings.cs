namespace DeliverySystem.Domain.Entities;

/// <summary>إعدادات النظام العامة (سجل واحد دائماً)</summary>
public class SystemSettings
{
    public int Id { get; set; }
    public string SystemName { get; set; } = "نظام ادارة المبيعات والتوصيل";
    public string? LogoPath { get; set; }
    public string? PrimaryColor { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Address { get; set; }
    public string? FooterText { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
