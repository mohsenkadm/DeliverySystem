using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Application.DTOs;

public class TransferOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int FromWarehouseId { get; set; }
    public string? FromWarehouseName { get; set; }
    public int ToWarehouseId { get; set; }
    public string? ToWarehouseName { get; set; }
    public int RequestedByEmployeeId { get; set; }
    public string? RequestedByEmployeeName { get; set; }
    public int? ApprovedByEmployeeId { get; set; }
    public string? ApprovedByEmployeeName { get; set; }
    public TransferOrderStatus Status { get; set; }
    public string StatusText => Status switch
    {
        TransferOrderStatus.Pending             => "معلق",
        TransferOrderStatus.AccountantApproved  => "موافقة المحاسب",
        TransferOrderStatus.WarehouseProcessing => "يعالجه المستودع",
        TransferOrderStatus.Completed           => "مكتمل",
        TransferOrderStatus.Rejected            => "مرفوض",
        TransferOrderStatus.ReturnPending        => "طلب إرجاع معلق",
        TransferOrderStatus.ReturnApproved       => "موافقة الإرجاع",
        TransferOrderStatus.ReturnCompleted      => "اكتمل الإرجاع",
        _                                        => ""
    };
    public TransferOrderType OrderType { get; set; }
    public string OrderTypeText => OrderType switch
    {
        TransferOrderType.OutboundToRepWarehouse => "إرسال لمستودع المندوب",
        TransferOrderType.ReturnToMainWarehouse  => "إرجاع للمستودع الرئيسي",
        _                                        => ""
    };
    public string? Notes { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<TransferOrderDetailDto> Details { get; set; } = [];
}

public class TransferOrderDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int RequestedQuantity { get; set; }
    public int? ApprovedQuantity { get; set; }
}

public class CreateTransferOrderDto
{
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public TransferOrderType OrderType { get; set; }
    public string? Notes { get; set; }
    public List<CreateTransferOrderDetailDto> Details { get; set; } = [];
}

public class CreateTransferOrderDetailDto
{
    public int ProductId { get; set; }
    public int RequestedQuantity { get; set; }
}

public class ApproveTransferOrderDto
{
    public int ApprovedByEmployeeId { get; set; }
    public List<ApproveTransferOrderDetailDto>? Details { get; set; }
}

public class ApproveTransferOrderDetailDto
{
    public int DetailId { get; set; }
    public int ApprovedQuantity { get; set; }
}
