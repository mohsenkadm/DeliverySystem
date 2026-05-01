namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات المخزون</summary>
public class InventoryDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
}

/// <summary>بيانات إنشاء/تعديل مخزون</summary>
public class CreateInventoryDto
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
}
