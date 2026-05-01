namespace DeliverySystem.Domain.Entities;

/// <summary>كيان المندوب</summary>
public class Representative
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>قائمة العملاء التابعين للمندوب</summary>
    public List<Customer> Customers { get; set; } = [];
}
