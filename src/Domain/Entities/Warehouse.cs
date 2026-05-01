namespace DeliverySystem.Domain.Entities;

/// <summary>كيان المستودع</summary>
public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? BranchId { get; set; }
    public Branch? BranchEntity { get; set; }

    /// <summary>إذا كان مستودعًا فرعيًا مخصصًا لمندوب</summary>
    public int? OwnerEmployeeId { get; set; }
    public Employee? OwnerEmployee { get; set; }
    public bool IsSubWarehouse { get; set; } = false;

    public List<Inventory> Inventories { get; set; } = [];
}
