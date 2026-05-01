using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Enums;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Invoices.Commands;

// ─── Create Invoice (with offer/promo support) ───────────────────────────────

public record CreateInvoiceCommand(CreateInvoiceDto Dto) : IRequest<InvoiceDto>;

public class CreateInvoiceCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken ct)
    {
        var details = request.Dto.Details.Select(d => new InvoiceDetail
        {
            ProductId = d.ProductId, Quantity = d.Quantity,
            UnitPrice = d.UnitPrice, Discount  = d.Discount
        }).ToList();

        // ── Apply offers ──────────────────────────────────────────────────────
        var offerSummaries = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Dto.PromoCode))
        {
            var now = DateTime.UtcNow;
            var offers = await uow.Offers.Query()
                .Where(o => o.IsActive
                    && o.PromoCode == request.Dto.PromoCode
                    && (o.StartDate == null || o.StartDate <= now)
                    && (o.EndDate   == null || o.EndDate   >= now))
                .ToListAsync(ct);

            foreach (var offer in offers)
            {
                var targets = offer.ProductId == null
                    ? details.ToList()
                    : details.Where(d => d.ProductId == offer.ProductId).ToList();

                foreach (var det in targets)
                {
                    if (offer.MinimumQuantity.HasValue && det.Quantity < offer.MinimumQuantity.Value)
                        continue;

                    switch (offer.OfferType)
                    {
                        case OfferType.BuyOneGetOne:
                            details.Add(new InvoiceDetail { ProductId = det.ProductId, Quantity = det.Quantity, UnitPrice = 0, Discount = 0 });
                            offerSummaries.Add($"عرض 1+1: {offer.Name}");
                            break;
                        case OfferType.BuyOneGetTwo:
                            details.Add(new InvoiceDetail { ProductId = det.ProductId, Quantity = det.Quantity * 2, UnitPrice = 0, Discount = 0 });
                            offerSummaries.Add($"عرض 1+2: {offer.Name}");
                            break;
                        case OfferType.BuyOneGetThree:
                            details.Add(new InvoiceDetail { ProductId = det.ProductId, Quantity = det.Quantity * 3, UnitPrice = 0, Discount = 0 });
                            offerSummaries.Add($"عرض 1+3: {offer.Name}");
                            break;
                        case OfferType.DiscountPercentage when offer.DiscountValue.HasValue:
                            det.Discount += det.UnitPrice * det.Quantity * (offer.DiscountValue.Value / 100m);
                            offerSummaries.Add($"خصم {offer.DiscountValue}%: {offer.Name}");
                            break;
                        case OfferType.FixedPrice when offer.DiscountValue.HasValue:
                            det.UnitPrice = offer.DiscountValue.Value;
                            offerSummaries.Add($"سعر ثابت {offer.DiscountValue} ر.س: {offer.Name}");
                            break;
                        case OfferType.CustomVariable when offer.DiscountValue.HasValue:
                            det.Discount += offer.DiscountValue.Value;
                            offerSummaries.Add($"خصم مخصص {offer.DiscountValue}: {offer.Name}");
                            break;
                    }
                }
            }
        }

        var total = details.Sum(d => d.UnitPrice * d.Quantity - d.Discount);
        var invoice = new Invoice
        {
            InvoiceNumber       = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            CustomerId          = request.Dto.CustomerId,
            EmployeeId          = request.Dto.EmployeeId,
            BranchId            = request.Dto.BranchId,
            InvoiceSource       = request.Dto.InvoiceSource,
            PromoCode           = request.Dto.PromoCode,
            AppliedOfferSummary = offerSummaries.Count > 0 ? string.Join("، ", offerSummaries) : null,
            TotalAmount         = total,
            PaidAmount          = 0,
            Status              = InvoiceStatus.Pending,
            PaymentStatus       = PaymentStatus.Unpaid,
            Details             = details
        };
        await uow.Invoices.AddAsync(invoice);
        await uow.SaveChangesAsync(ct);
        return MapToDto(invoice);
    }

    private static InvoiceDto MapToDto(Invoice i) => new()
    {
        Id = i.Id, InvoiceNumber = i.InvoiceNumber, OrderDate = i.OrderDate,
        CustomerId = i.CustomerId, EmployeeId = i.EmployeeId, BranchId = i.BranchId,
        InvoiceSource = i.InvoiceSource, ApprovedByEmployeeId = i.ApprovedByEmployeeId, ApprovedAt = i.ApprovedAt,
        PromoCode = i.PromoCode, AppliedOfferSummary = i.AppliedOfferSummary,
        TotalAmount = i.TotalAmount, PaidAmount = i.PaidAmount,
        RemainingAmount = i.TotalAmount - i.PaidAmount,
        Status = i.Status, PaymentStatus = i.PaymentStatus,
        StatusText = GetStatusText(i.Status), PaymentStatusText = GetPaymentText(i.PaymentStatus)
    };

    private static string GetStatusText(InvoiceStatus s) => s switch
    {
        InvoiceStatus.Pending             => "معلق",
        InvoiceStatus.Deferred            => "مؤجل",
        InvoiceStatus.AwaitingDelivery    => "في انتظار التوصيل",
        InvoiceStatus.Completed           => "مكتمل",
        InvoiceStatus.Rejected            => "مرفوض",
        InvoiceStatus.Accepted            => "مقبول",
        InvoiceStatus.WarehouseProcessing => "قيد التجهيز في المستودع",
        InvoiceStatus.Delivered           => "تم التسليم",
        _                                 => string.Empty
    };

    private static string GetPaymentText(PaymentStatus s) => s switch
    {
        PaymentStatus.Unpaid      => "غير مدفوع",
        PaymentStatus.PartialPaid => "مدفوع جزئياً",
        PaymentStatus.FullPaid    => "مدفوع بالكامل",
        _                         => string.Empty
    };
}

// ─── Pay Invoice ──────────────────────────────────────────────────────────────

public record PayInvoiceCommand(int InvoiceId, decimal Amount) : IRequest<bool>;

public class PayInvoiceCommandHandler(IUnitOfWork uow) : IRequestHandler<PayInvoiceCommand, bool>
{
    public async Task<bool> Handle(PayInvoiceCommand request, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice is null) return false;
        invoice.PaidAmount    = Math.Min(invoice.TotalAmount, invoice.PaidAmount + request.Amount);
        invoice.PaymentStatus = invoice.PaidAmount >= invoice.TotalAmount
            ? PaymentStatus.FullPaid
            : invoice.PaidAmount > 0 ? PaymentStatus.PartialPaid : PaymentStatus.Unpaid;
        uow.Invoices.Update(invoice);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

// ─── Update Invoice Status ────────────────────────────────────────────────────

public record UpdateInvoiceStatusCommand(int InvoiceId, InvoiceStatus Status) : IRequest<bool>;

public class UpdateInvoiceStatusCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateInvoiceStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateInvoiceStatusCommand request, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice is null) return false;
        invoice.Status = request.Status;
        uow.Invoices.Update(invoice);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

// ─── Assign Employee ──────────────────────────────────────────────────────────

public record AssignDriverToInvoiceCommand(int InvoiceId, int EmployeeId) : IRequest<bool>;

public class AssignDriverToInvoiceCommandHandler(IUnitOfWork uow) : IRequestHandler<AssignDriverToInvoiceCommand, bool>
{
    public async Task<bool> Handle(AssignDriverToInvoiceCommand request, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice is null) return false;
        invoice.EmployeeId = request.EmployeeId;
        invoice.Status     = InvoiceStatus.AwaitingDelivery;
        uow.Invoices.Update(invoice);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

// ─── Get All Invoices ─────────────────────────────────────────────────────────

public record GetAllInvoicesQuery(
    string?        InvoiceNumber = null,
    int?           CustomerId    = null,
    int?           EmployeeId    = null,
    int?           BranchId      = null,
    InvoiceStatus? Status        = null,
    DateTime?      From          = null,
    DateTime?      To            = null,
    int?           DriverId      = null)   // kept for legacy API compat
    : IRequest<IEnumerable<InvoiceDto>>;

public class GetAllInvoicesQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetAllInvoicesQuery, IEnumerable<InvoiceDto>>
{
    public async Task<IEnumerable<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken ct)
    {
        var query = uow.Invoices.Query()
            .Include(i => i.Customer)
            .Include(i => i.Employee)
            .Include(i => i.ApprovedByEmployee)
            .Include(i => i.Details).ThenInclude(d => d.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.InvoiceNumber))
            query = query.Where(i => i.InvoiceNumber.Contains(request.InvoiceNumber));
        if (request.CustomerId.HasValue)  query = query.Where(i => i.CustomerId == request.CustomerId);
        if (request.EmployeeId.HasValue)  query = query.Where(i => i.EmployeeId == request.EmployeeId);
        if (request.DriverId.HasValue)    query = query.Where(i => i.EmployeeId == request.DriverId);
        if (request.BranchId.HasValue)    query = query.Where(i => i.BranchId   == request.BranchId);
        if (request.Status.HasValue)      query = query.Where(i => i.Status      == request.Status);
        if (request.From.HasValue)        query = query.Where(i => i.OrderDate  >= request.From);
        if (request.To.HasValue)          query = query.Where(i => i.OrderDate  <= request.To);

        var invoices = await query.OrderByDescending(i => i.OrderDate).ToListAsync(ct);
        return invoices.Select(MapRow);
    }

    private static InvoiceDto MapRow(Invoice i) => new()
    {
        Id = i.Id, InvoiceNumber = i.InvoiceNumber, OrderDate = i.OrderDate,
        CustomerId = i.CustomerId, CustomerName = i.Customer?.FullName,
        EmployeeId = i.EmployeeId, EmployeeName = i.Employee?.FullName,
        ApprovedByEmployeeId = i.ApprovedByEmployeeId, ApprovedByEmployeeName = i.ApprovedByEmployee?.FullName,
        ApprovedAt = i.ApprovedAt, InvoiceSource = i.InvoiceSource,
        BranchId = i.BranchId, PromoCode = i.PromoCode,
        AppliedOfferSummary = i.AppliedOfferSummary,
        TotalAmount = i.TotalAmount, PaidAmount = i.PaidAmount,
        RemainingAmount = i.TotalAmount - i.PaidAmount,
        Status = i.Status, PaymentStatus = i.PaymentStatus,
        StatusText = StatusText(i.Status), PaymentStatusText = PayText(i.PaymentStatus),
        Details = i.Details.Select(d => new InvoiceDetailDto
        {
            Id = d.Id, ProductId = d.ProductId, ProductName = d.Product?.Name,
            Quantity = d.Quantity, UnitPrice = d.UnitPrice,
            Discount = d.Discount, SubTotal = d.UnitPrice * d.Quantity - d.Discount
        }).ToList()
    };

    private static string StatusText(InvoiceStatus s) => s switch
    {
        InvoiceStatus.Pending             => "معلق",
        InvoiceStatus.Deferred            => "مؤجل",
        InvoiceStatus.AwaitingDelivery    => "في انتظار التوصيل",
        InvoiceStatus.Completed           => "مكتمل",
        InvoiceStatus.Rejected            => "مرفوض",
        InvoiceStatus.Accepted            => "مقبول",
        InvoiceStatus.WarehouseProcessing => "قيد التجهيز في المستودع",
        InvoiceStatus.Delivered           => "تم التسليم",
        _                                 => ""
    };
    private static string PayText(PaymentStatus s) => s switch
    {
        PaymentStatus.Unpaid      => "غير مدفوع",
        PaymentStatus.PartialPaid => "مدفوع جزئياً",
        PaymentStatus.FullPaid    => "مدفوع بالكامل",
        _                         => ""
    };
}

// ─── Get Invoice By Id ────────────────────────────────────────────────────────

public record GetInvoiceByIdQuery(int Id) : IRequest<InvoiceDto?>;

public class GetInvoiceByIdQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken ct)
    {
        var i = await uow.Invoices.Query()
            .Include(x => x.Customer).Include(x => x.Employee).Include(x => x.ApprovedByEmployee)
            .Include(x => x.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (i is null) return null;
        return new InvoiceDto
        {
            Id = i.Id, InvoiceNumber = i.InvoiceNumber, OrderDate = i.OrderDate,
            CustomerId = i.CustomerId, CustomerName = i.Customer?.FullName,
            EmployeeId = i.EmployeeId, EmployeeName = i.Employee?.FullName,
            ApprovedByEmployeeId = i.ApprovedByEmployeeId, ApprovedByEmployeeName = i.ApprovedByEmployee?.FullName,
            ApprovedAt = i.ApprovedAt, InvoiceSource = i.InvoiceSource,
            BranchId = i.BranchId, PromoCode = i.PromoCode,
            AppliedOfferSummary = i.AppliedOfferSummary,
            TotalAmount = i.TotalAmount, PaidAmount = i.PaidAmount,
            RemainingAmount = i.TotalAmount - i.PaidAmount,
            Status = i.Status, PaymentStatus = i.PaymentStatus,
            StatusText = StatusText(i.Status), PaymentStatusText = PayText(i.PaymentStatus),
            Details = i.Details.Select(d => new InvoiceDetailDto
            {
                Id = d.Id, ProductId = d.ProductId, ProductName = d.Product?.Name,
                Quantity = d.Quantity, UnitPrice = d.UnitPrice,
                Discount = d.Discount, SubTotal = d.UnitPrice * d.Quantity - d.Discount
            }).ToList()
        };
    }

    private static string StatusText(InvoiceStatus s) => s switch
    {
        InvoiceStatus.Pending             => "معلق",
        InvoiceStatus.Deferred            => "مؤجل",
        InvoiceStatus.AwaitingDelivery    => "في انتظار التوصيل",
        InvoiceStatus.Completed           => "مكتمل",
        InvoiceStatus.Rejected            => "مرفوض",
        InvoiceStatus.Accepted            => "مقبول",
        InvoiceStatus.WarehouseProcessing => "قيد التجهيز في المستودع",
        InvoiceStatus.Delivered           => "تم التسليم",
        _                                 => ""
    };
    private static string PayText(PaymentStatus s) => s switch
    {
        PaymentStatus.Unpaid      => "غير مدفوع",
        PaymentStatus.PartialPaid => "مدفوع جزئياً",
        PaymentStatus.FullPaid    => "مدفوع بالكامل",
        _                         => ""
    };
}
