namespace DeliverySystem.Domain.Entities;

public class SalesReturnDetail
{
    public int Id { get; set; }
    public int SalesReturnId { get; set; }
    public SalesReturn? SalesReturn { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}
