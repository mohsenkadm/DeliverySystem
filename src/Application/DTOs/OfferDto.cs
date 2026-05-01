using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Application.DTOs;

public class OfferDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public OfferType OfferType { get; set; }
    public string OfferTypeText => OfferType switch
    {
        OfferType.BuyOneGetOne       => "اشتر 1 واحصل على 1 مجاناً",
        OfferType.BuyOneGetTwo       => "اشتر 1 واحصل على 2 مجاناً",
        OfferType.BuyOneGetThree     => "اشتر 1 واحصل على 3 مجاناً",
        OfferType.DiscountPercentage => "خصم بالنسبة المئوية",
        OfferType.FixedPrice         => "سعر ثابت",
        OfferType.CustomVariable     => "عرض مخصص",
        _                            => "غير معروف"
    };
    public decimal? DiscountValue { get; set; }
    public int? FreeQuantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public string? PromoCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsCurrentlyValid =>
        IsActive &&
        (StartDate == null || StartDate <= DateTime.UtcNow) &&
        (EndDate   == null || EndDate   >= DateTime.UtcNow);
}

public class CreateOfferDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ProductId { get; set; }
    public OfferType OfferType { get; set; }
    public decimal? DiscountValue { get; set; }
    public int? FreeQuantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public string? PromoCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateOfferDto : CreateOfferDto { }
