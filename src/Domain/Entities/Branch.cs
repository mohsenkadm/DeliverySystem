namespace DeliverySystem.Domain.Entities;

public class Branch
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Region { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Customer> Customers { get; set; } = [];
    public List<Employee> Employees { get; set; } = [];
    public List<Invoice> Invoices { get; set; } = [];
    public List<Warehouse> Warehouses { get; set; } = [];
}
