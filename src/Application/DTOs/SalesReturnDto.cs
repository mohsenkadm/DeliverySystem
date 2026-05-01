using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Application.DTOs;

public class SalesReturnDto
{
    public int Id { get; set; }
    public int? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public int RequestedByEmployeeId { get; set; }
    public string? RequestedByEmployeeName { get; set; }
    public int? ApprovedByManagerId { get; set; }
    public string? ApprovedByManagerName { get; set; }
    public SalesReturnStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public string? Notes { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<SalesReturnDetailDto> Details { get; set; } = [];
}

public class SalesReturnDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateSalesReturnDto
{
    public int? InvoiceId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<SalesReturnItemDto> Items { get; set; } = [];
}

public class SalesReturnItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}
