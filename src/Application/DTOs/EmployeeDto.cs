namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات الموظف</summary>
public class EmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string EmployeeType { get; set; } = "Individual";
    public string? Roles { get; set; }
    public string? AssignedAreas { get; set; }
    public string? CarNumber { get; set; }
    public string? CarType { get; set; }
    public string? Region { get; set; }
    public string? Branch { get; set; }
    public string? IdImagePath { get; set; }
    public string? PhotoPath { get; set; }
    public int CustomerCount { get; set; }
    public int ActiveDeliveries { get; set; }
    public int CompletedDeliveries { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalDebt { get; set; }

    public List<string> RolesList =>
        string.IsNullOrEmpty(Roles) ? [] : [.. Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

    public List<string> AreasList =>
        string.IsNullOrEmpty(AssignedAreas) ? [] : [.. AssignedAreas.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
}

/// <summary>بيانات إنشاء موظف جديد</summary>
public class CreateEmployeeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string EmployeeType { get; set; } = "Individual";
    public List<string> SelectedRoles { get; set; } = [];
    public List<string> SelectedAreas { get; set; } = [];
    public string? CarNumber { get; set; }
    public string? CarType { get; set; }
    public string? Region { get; set; }
    public string? Branch { get; set; }
    public string? IdImagePath { get; set; }
    public string? PhotoPath { get; set; }
}

/// <summary>بيانات تعديل موظف</summary>
public class UpdateEmployeeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string EmployeeType { get; set; } = "Individual";
    public List<string> SelectedRoles { get; set; } = [];
    public List<string> SelectedAreas { get; set; } = [];
    public string? CarNumber { get; set; }
    public string? CarType { get; set; }
    public string? Region { get; set; }
    public string? Branch { get; set; }
    public string? IdImagePath { get; set; }
    public string? PhotoPath { get; set; }
}

/// <summary>بيانات تسجيل دخول الموظف</summary>
public class EmployeeLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
