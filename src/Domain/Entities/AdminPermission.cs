namespace DeliverySystem.Domain.Entities;

/// <summary>كيان صلاحيات المسؤول لكل صفحة</summary>
public class AdminPermission
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanView { get; set; }

    /// <summary>المسؤول المرتبط بهذه الصلاحية</summary>
    public Admin Admin { get; set; } = null!;
}
