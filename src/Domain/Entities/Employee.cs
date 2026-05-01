namespace DeliverySystem.Domain.Entities;

/// <summary>كيان الموظف (يجمع السائقين والمندوبين وجميع أنواع الموظفين)</summary>
public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>نوع الموظف: فرد / مندوب / جملة</summary>
    public string EmployeeType { get; set; } = "Individual";

    /// <summary>الأدوار الوظيفية (مفصولة بفاصلة)</summary>
    public string? Roles { get; set; }

    /// <summary>المناطق المعينة (مفصولة بفاصلة)</summary>
    public string? AssignedAreas { get; set; }

    public string? CarNumber { get; set; }
    public string? CarType { get; set; }
    public string? Region { get; set; }
    public string? Branch { get; set; }
    public string? IdImagePath { get; set; }
    public string? PhotoPath { get; set; }

    public int? BranchId { get; set; }
    public Branch? BranchEntity { get; set; }

    /// <summary>معرف المشرف المباشر (للمندوبين)</summary>
    public int? SupervisorId { get; set; }
    public Employee? Supervisor { get; set; }

    public List<Invoice> Invoices { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
}
