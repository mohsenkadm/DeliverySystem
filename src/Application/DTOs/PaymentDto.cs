using DeliverySystem.Domain.Enums;

namespace DeliverySystem.Application.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public int? InvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? PaidByEmployeeId { get; set; }
    public string? PaidByEmployeeName { get; set; }
    public int? ReceivedByEmployeeId { get; set; }
    public string? ReceivedByEmployeeName { get; set; }
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public string TypeText => Type switch
    {
        PaymentType.CustomerToDriver         => "عميل → سائق",
        PaymentType.CustomerToRepresentative => "عميل → مندوب",
        PaymentType.DriverToCompany          => "سائق → شركة",
        PaymentType.RepresentativeToCompany  => "مندوب → شركة",
        _                                    => ""
    };
    public string? Notes { get; set; }
    public bool IsVerified { get; set; }
    public DateTime PaidAt { get; set; }
}

public class CreatePaymentDto
{
    public int? InvoiceId { get; set; }
    public int? CustomerId { get; set; }
    public int? PaidByEmployeeId { get; set; }
    public int? ReceivedByEmployeeId { get; set; }
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public string? Notes { get; set; }
}
