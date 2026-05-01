using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Enums;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Payments.Commands;

// ── Mapper ────────────────────────────────────────────────────────────────────

public static class PaymentMapper
{
    public static PaymentDto Map(Payment p) => new()
    {
        Id = p.Id,
        InvoiceId = p.InvoiceId, InvoiceNumber = p.Invoice?.InvoiceNumber,
        CustomerId = p.CustomerId, CustomerName = p.Customer?.FullName,
        PaidByEmployeeId = p.PaidByEmployeeId, PaidByEmployeeName = p.PaidByEmployee?.FullName,
        ReceivedByEmployeeId = p.ReceivedByEmployeeId, ReceivedByEmployeeName = p.ReceivedByEmployee?.FullName,
        Amount = p.Amount, Type = p.Type, Notes = p.Notes,
        IsVerified = p.IsVerified, PaidAt = p.PaidAt
    };
}

// ── Queries ───────────────────────────────────────────────────────────────────

public record GetAllPaymentsQuery(
    int? EmployeeId = null,
    int? CustomerId = null,
    int? InvoiceId = null,
    bool? IsVerified = null) : IRequest<IEnumerable<PaymentDto>>;

public class GetAllPaymentsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetAllPaymentsQuery, IEnumerable<PaymentDto>>
{
    public async Task<IEnumerable<PaymentDto>> Handle(GetAllPaymentsQuery request, CancellationToken ct)
    {
        IQueryable<Payment> q = uow.Payments.Query()
            .Include(p => p.Invoice).Include(p => p.Customer)
            .Include(p => p.PaidByEmployee).Include(p => p.ReceivedByEmployee);

        if (request.EmployeeId.HasValue)
            q = q.Where(p => p.PaidByEmployeeId == request.EmployeeId || p.ReceivedByEmployeeId == request.EmployeeId);
        if (request.CustomerId.HasValue)
            q = q.Where(p => p.CustomerId == request.CustomerId);
        if (request.InvoiceId.HasValue)
            q = q.Where(p => p.InvoiceId == request.InvoiceId);
        if (request.IsVerified.HasValue)
            q = q.Where(p => p.IsVerified == request.IsVerified.Value);

        var list = await q.OrderByDescending(p => p.PaidAt).ToListAsync(ct);
        return list.Select(PaymentMapper.Map);
    }
}

// ── Commands ──────────────────────────────────────────────────────────────────

/// Driver/rep records cash collected from customer
public record RecordPaymentCommand(CreatePaymentDto Dto) : IRequest<PaymentDto>;

public class RecordPaymentCommandHandler(IUnitOfWork uow)
    : IRequestHandler<RecordPaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        var payment = new Payment
        {
            InvoiceId = request.Dto.InvoiceId,
            CustomerId = request.Dto.CustomerId,
            PaidByEmployeeId = request.Dto.PaidByEmployeeId,
            ReceivedByEmployeeId = request.Dto.ReceivedByEmployeeId,
            Amount = request.Dto.Amount,
            Type = request.Dto.Type,
            Notes = request.Dto.Notes
        };
        await uow.Payments.AddAsync(payment);

        // Update invoice PaidAmount if linked
        if (request.Dto.InvoiceId.HasValue && (request.Dto.Type == PaymentType.DriverToCompany || request.Dto.Type == PaymentType.RepresentativeToCompany))
        {
            var invoice = await uow.Invoices.GetByIdAsync(request.Dto.InvoiceId.Value);
            if (invoice is not null)
            {
                invoice.PaidAmount = Math.Min(invoice.TotalAmount, invoice.PaidAmount + request.Dto.Amount);
                invoice.PaymentStatus = invoice.PaidAmount >= invoice.TotalAmount
                    ? PaymentStatus.FullPaid
                    : invoice.PaidAmount > 0 ? PaymentStatus.PartialPaid : PaymentStatus.Unpaid;
                if (invoice.PaymentStatus == PaymentStatus.FullPaid && invoice.Status == InvoiceStatus.Delivered)
                    invoice.Status = InvoiceStatus.Completed;
            }
        }

        await uow.SaveChangesAsync(ct);
        return PaymentMapper.Map(payment);
    }
}

/// Cashier verifies a payment (marks it verified)
public record VerifyPaymentCommand(int PaymentId, int ReceivedByEmployeeId) : IRequest<bool>;

public class VerifyPaymentCommandHandler(IUnitOfWork uow)
    : IRequestHandler<VerifyPaymentCommand, bool>
{
    public async Task<bool> Handle(VerifyPaymentCommand request, CancellationToken ct)
    {
        var payment = await uow.Payments.GetByIdAsync(request.PaymentId);
        if (payment is null || payment.IsVerified) return false;
        payment.IsVerified = true;
        payment.ReceivedByEmployeeId = request.ReceivedByEmployeeId;
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
