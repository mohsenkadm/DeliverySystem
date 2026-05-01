namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات المندوب</summary>
public class RepresentativeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CustomerCount { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalDebt { get; set; }
}

/// <summary>بيانات إنشاء مندوب جديد</summary>
public class CreateRepresentativeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>بيانات تعديل مندوب</summary>
public class UpdateRepresentativeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

/// <summary>بيانات تسجيل دخول المندوب</summary>
public class RepresentativeLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
