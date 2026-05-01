namespace DeliverySystem.Domain.Entities;

/// <summary>كيان المسؤول (الأدمن)</summary>
public class Admin
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>قائمة صلاحيات المسؤول</summary>
    public List<AdminPermission> Permissions { get; set; } = [];
}
