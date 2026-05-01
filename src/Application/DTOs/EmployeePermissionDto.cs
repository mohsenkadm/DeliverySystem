namespace DeliverySystem.Application.DTOs;

public class EmployeePermissionDto
{
    public int Id { get; set; }
    public string PageName { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
