namespace DeliverySystem.Domain.Entities;

/// <summary>كيان العميل</summary>
public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? StoreName { get; set; }
    public string? Description { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    /// <summary>نوع العميل: فرد / جملة</summary>
    public string ClientType { get; set; } = "Individual";

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Region { get; set; }
    public string? Branch { get; set; }
    public string? StoreImagePath { get; set; }

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>معرف الموظف المندوب المسؤول عن هذا العميل (اختياري)</summary>
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? BranchId { get; set; }
    public Branch? BranchEntity { get; set; }

    public List<Invoice> Invoices { get; set; } = [];
}
