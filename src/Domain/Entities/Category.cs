namespace DeliverySystem.Domain.Entities;

/// <summary>كيان تصنيف المنتجات</summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>قائمة المنتجات في هذا التصنيف</summary>
    public List<Product> Products { get; set; } = [];
}
