namespace DeliverySystem.Domain.Enums;

public enum PaymentType
{
    CustomerToDriver = 0,         // العميل يدفع للسائق
    CustomerToRepresentative = 1, // العميل يدفع للمندوب
    DriverToCompany = 2,          // السائق يدفع للشركة (كاشير)
    RepresentativeToCompany = 3   // المندوب يدفع للشركة (كاشير)
}
