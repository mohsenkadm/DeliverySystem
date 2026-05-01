using System.ComponentModel.DataAnnotations;

namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات المسؤول</summary>
public class AdminDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AdminPermissionDto> Permissions { get; set; } = [];
}

/// <summary>نقل بيانات صلاحيات المسؤول</summary>
public class AdminPermissionDto
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanView { get; set; }
}

/// <summary>بيانات إنشاء مسؤول جديد</summary>
public class CreateAdminDto
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>بيانات تسجيل دخول المسؤول</summary>
public class AdminLoginDto
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    public string Password { get; set; } = string.Empty;
}
