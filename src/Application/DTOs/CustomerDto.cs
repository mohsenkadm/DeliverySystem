namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات العميل</summary>
public class CustomerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? StoreName { get; set; }
    public string? Description { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ClientType { get; set; } = "Individual";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Region { get; set; }
    public string? Branch { get; set; }
    public string? StoreImagePath { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int InvoiceCount { get; set; }
    public decimal TotalInvoices { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalDebt { get; set; }
}

/// <summary>بيانات إنشاء عميل جديد</summary>
public class CreateCustomerDto
{
    public string FullName { get; set; } = string.Empty;
    public string? StoreName { get; set; }
    public string? Description { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ClientType { get; set; } = "Individual";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Region { get; set; }
    public string? Branch { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? StoreImagePath { get; set; }
}

/// <summary>بيانات تعديل عميل</summary>
public class UpdateCustomerDto
{
    public string FullName { get; set; } = string.Empty;
    public string? StoreName { get; set; }
    public string? Description { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ClientType { get; set; } = "Individual";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Region { get; set; }
    public string? Branch { get; set; }
    public int? EmployeeId { get; set; }
    public string? StoreImagePath { get; set; }
}

/// <summary>بيانات تسجيل دخول العميل</summary>
public class CustomerLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
