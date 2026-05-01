using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Domain.Entities;

/// <summary>طلب نقل البضاعة بين المستودعات</summary>
public class TransferOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public int FromWarehouseId { get; set; }
    public Warehouse? FromWarehouse { get; set; }

    public int ToWarehouseId { get; set; }
    public Warehouse? ToWarehouse { get; set; }

    public int RequestedByEmployeeId { get; set; }
    public Employee? RequestedByEmployee { get; set; }

    public int? ApprovedByEmployeeId { get; set; }
    public Employee? ApprovedByEmployee { get; set; }

    public TransferOrderStatus Status { get; set; } = TransferOrderStatus.Pending;
    public TransferOrderType OrderType { get; set; }
    public string? Notes { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<TransferOrderDetail> Details { get; set; } = [];
}
