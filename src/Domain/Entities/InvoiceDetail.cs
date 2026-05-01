namespace DeliverySystem.Domain.Entities;

/// <summary>كيان تفاصيل الفاتورة (المنتجات والكميات)</summary>
public class InvoiceDetail
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }

    /// <summary>المجموع الفرعي للصنف بعد الخصم</summary>
    public decimal SubTotal => (UnitPrice * Quantity) - Discount;
}
