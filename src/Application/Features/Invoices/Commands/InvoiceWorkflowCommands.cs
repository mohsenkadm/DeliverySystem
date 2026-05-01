using DeliverySystem.Domain.Enums;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;

namespace DeliverySystem.Application.Features.Invoices.Commands;

// ── Account Manager Approval ──────────────────────────────────────────────────

public record ApproveInvoiceCommand(int InvoiceId, int ApprovedByEmployeeId) : IRequest<bool>;

public class ApproveInvoiceCommandHandler(IUnitOfWork uow)
    : IRequestHandler<ApproveInvoiceCommand, bool>
{
    public async Task<bool> Handle(ApproveInvoiceCommand request, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice is null || invoice.Status != InvoiceStatus.Pending) return false;

        invoice.Status = InvoiceStatus.Accepted;
        invoice.ApprovedByEmployeeId = request.ApprovedByEmployeeId;
        invoice.ApprovedAt = DateTime.UtcNow;

        // Create customer debt notification
        await uow.Notifications.AddAsync(new Notification
        {
            Title = "فاتورة جديدة مقبولة",
            Body = $"تمت الموافقة على الفاتورة وتسجيلها كدين على العميل",
            Target = NotificationTarget.Customer,
            TargetUserId = invoice.CustomerId
        });

        await uow.SaveChangesAsync(ct);
        return true;
    }
}

// ── Reject Invoice ────────────────────────────────────────────────────────────

public record RejectInvoiceCommand(int InvoiceId, int RejectedByEmployeeId) : IRequest<bool>;

public class RejectInvoiceCommandHandler(IUnitOfWork uow)
    : IRequestHandler<RejectInvoiceCommand, bool>
{
    public async Task<bool> Handle(RejectInvoiceCommand request, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice is null) return false;

        invoice.Status = InvoiceStatus.Rejected;

        await uow.Notifications.AddAsync(new Notification
        {
            Title = "تم رفض الفاتورة",
            Body = "تم رفض الفاتورة من قِبل مدير الحسابات",
            Target = NotificationTarget.Customer,
            TargetUserId = invoice.CustomerId
        });

        await uow.SaveChangesAsync(ct);
        return true;
    }
}

// ── Warehouse Processing ──────────────────────────────────────────────────────

public record StartInvoiceWarehouseProcessingCommand(int InvoiceId) : IRequest<bool>;

public class StartInvoiceWarehouseProcessingCommandHandler(IUnitOfWork uow)
    : IRequestHandler<StartInvoiceWarehouseProcessingCommand, bool>
{
    public async Task<bool> Handle(StartInvoiceWarehouseProcessingCommand request, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice is null || invoice.Status != InvoiceStatus.Accepted) return false;

        invoice.Status = InvoiceStatus.WarehouseProcessing;

        await uow.Notifications.AddAsync(new Notification
        {
            Title = "طلبك قيد التجهيز",
            Body = "المستودع يجهز طلبك حالياً",
            Target = NotificationTarget.Customer,
            TargetUserId = invoice.CustomerId
        });

        await uow.SaveChangesAsync(ct);
        return true;
    }
}

// ── Assign Driver → AwaitingDelivery ─────────────────────────────────────────

public record AssignDriverAndDispatchCommand(int InvoiceId, int DriverEmployeeId) : IRequest<bool>;

public class AssignDriverAndDispatchCommandHandler(IUnitOfWork uow)
    : IRequestHandler<AssignDriverAndDispatchCommand, bool>
{
    public async Task<bool> Handle(AssignDriverAndDispatchCommand request, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice is null || invoice.Status != InvoiceStatus.WarehouseProcessing) return false;

        invoice.EmployeeId = request.DriverEmployeeId;
        invoice.Status = InvoiceStatus.AwaitingDelivery;

        await uow.Notifications.AddAsync(new Notification
        {
            Title = "طلبك في الطريق",
            Body = "السائق في الطريق لتوصيل طلبك",
            Target = NotificationTarget.Customer,
            TargetUserId = invoice.CustomerId
        });

        await uow.SaveChangesAsync(ct);
        return true;
    }
}

// ── Driver Confirms Delivery ──────────────────────────────────────────────────

public record ConfirmInvoiceDeliveredCommand(int InvoiceId) : IRequest<bool>;

public class ConfirmInvoiceDeliveredCommandHandler(IUnitOfWork uow)
    : IRequestHandler<ConfirmInvoiceDeliveredCommand, bool>
{
    public async Task<bool> Handle(ConfirmInvoiceDeliveredCommand request, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice is null || invoice.Status != InvoiceStatus.AwaitingDelivery) return false;

        invoice.Status = InvoiceStatus.Delivered;

        await uow.Notifications.AddAsync(new Notification
        {
            Title = "تم تسليم طلبك",
            Body = "تم تسليم طلبك. يرجى تسوية المبلغ المستحق",
            Target = NotificationTarget.Customer,
            TargetUserId = invoice.CustomerId
        });

        await uow.SaveChangesAsync(ct);
        return true;
    }
}
