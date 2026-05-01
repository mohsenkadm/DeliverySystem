namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات تصنيف المنتجات</summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>بيانات إنشاء/تعديل تصنيف</summary>
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
}
