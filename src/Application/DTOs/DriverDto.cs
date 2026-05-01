namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات السائق</summary>
public class DriverDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ActiveDeliveries { get; set; }
    public int CompletedDeliveries { get; set; }
}

/// <summary>بيانات إنشاء سائق جديد</summary>
public class CreateDriverDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>بيانات تعديل سائق</summary>
public class UpdateDriverDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>بيانات تسجيل دخول السائق</summary>
public class DriverLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
