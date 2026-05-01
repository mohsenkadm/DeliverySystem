namespace DeliverySystem.Application.DTOs;

public class SystemSettingsDto
{
    public int Id { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string? PrimaryColor { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Address { get; set; }
    public string? FooterText { get; set; }
    public DateTime UpdatedAt { get; set; }
}
