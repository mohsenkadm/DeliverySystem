namespace DeliverySystem.Domain.Entities;

/// <summary>كيان المخزون (ربط المنتج بالمستودع)</summary>
public class Inventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
