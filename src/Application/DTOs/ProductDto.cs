namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات المنتج</summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string? CartonType { get; set; }
    public int? BaseQuantity { get; set; }
    public DateTime? ProductionDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? ImagePath { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductInventoryDto> Inventories { get; set; } = [];
}

/// <summary>بيانات مخزون المنتج في كل مستودع</summary>
public class ProductInventoryDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

/// <summary>بيانات إنشاء منتج جديد</summary>
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string? CartonType { get; set; }
    public int? BaseQuantity { get; set; }
    public DateTime? ProductionDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? ImagePath { get; set; }
    public int CategoryId { get; set; }
}

/// <summary>بيانات تعديل منتج</summary>
public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string? CartonType { get; set; }
    public int? BaseQuantity { get; set; }
    public DateTime? ProductionDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? ImagePath { get; set; }
    public int CategoryId { get; set; }
}
