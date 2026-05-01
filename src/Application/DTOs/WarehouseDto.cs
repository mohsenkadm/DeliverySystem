namespace DeliverySystem.Application.DTOs;

/// <summary>نقل بيانات المستودع</summary>
public class WarehouseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>بيانات إنشاء/تعديل مستودع</summary>
public class CreateWarehouseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
}
