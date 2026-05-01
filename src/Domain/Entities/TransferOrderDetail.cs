namespace DeliverySystem.Domain.Entities;

/// <summary>تفاصيل أصناف طلب النقل</summary>
public class TransferOrderDetail
{
    public int Id { get; set; }

    public int TransferOrderId { get; set; }
    public TransferOrder? TransferOrder { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int RequestedQuantity { get; set; }
    public int? ApprovedQuantity { get; set; }
}
