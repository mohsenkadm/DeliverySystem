namespace DeliverySystem.Domain.Enums;

public enum TransferOrderType
{
    OutboundToRepWarehouse = 0, // من المستودع الرئيسي إلى مستودع المندوب
    ReturnToMainWarehouse = 1   // من مستودع المندوب إلى المستودع الرئيسي
}
