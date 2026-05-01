using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Domain.Entities;

public class Offer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public OfferType OfferType { get; set; }

    /// Percentage for DiscountPercentage, price for FixedPrice
    public decimal? DiscountValue { get; set; }

    /// Free quantity added for BuyOneGetX offers (1,2,3) or custom
    public int? FreeQuantity { get; set; }

    /// Minimum purchased quantity to trigger the offer
    public int? MinimumQuantity { get; set; }

    public string? PromoCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
