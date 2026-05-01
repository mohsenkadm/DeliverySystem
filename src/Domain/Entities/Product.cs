namespace DeliverySystem.Domain.Entities;

/// <summary>كيان المنتج</summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal DiscountPercentage { get; set; }

    /// <summary>نوع الكرتون/وحدة القياس: صندوق / غرام / وحدة</summary>
    public string? CartonType { get; set; }

    /// <summary>الكمية الأساسية في الكرتون</summary>
    public int? BaseQuantity { get; set; }

    public DateTime? ProductionDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public List<Inventory> Inventories { get; set; } = [];
    public List<InvoiceDetail> InvoiceDetails { get; set; } = [];
}
