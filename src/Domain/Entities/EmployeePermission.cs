namespace DeliverySystem.Domain.Entities;

/// <summary>صلاحيات الموظف على واجهات لوحة التحكم</summary>
public class EmployeePermission
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }

    public Employee? Employee { get; set; }
}
