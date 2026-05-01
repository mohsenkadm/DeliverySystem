using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Enums;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.SalesReturns.Commands;

// ── Get All ───────────────────────────────────────────────────────────────────

public record GetAllSalesReturnsQuery(SalesReturnStatus? Status = null) : IRequest<List<SalesReturnDto>>;

public class GetAllSalesReturnsQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAllSalesReturnsQuery, List<SalesReturnDto>>
{
    public async Task<List<SalesReturnDto>> Handle(GetAllSalesReturnsQuery request, CancellationToken ct)
    {
        IQueryable<SalesReturn> q = uow.SalesReturns.Query()
            .Include(r => r.Invoice)
            .Include(r => r.RequestedByEmployee)
            .Include(r => r.ApprovedByManager)
            .Include(r => r.Details).ThenInclude(d => d.Product);

        if (request.Status.HasValue) q = q.Where(r => r.Status == request.Status.Value);

        return await q.OrderByDescending(r => r.RequestedAt)
            .Select(r => Map(r)).ToListAsync(ct);
    }

    internal static SalesReturnDto Map(SalesReturn r) => new()
    {
        Id = r.Id, InvoiceId = r.InvoiceId, InvoiceNumber = r.Invoice != null ? r.Invoice.InvoiceNumber : null,
        RequestedByEmployeeId = r.RequestedByEmployeeId, RequestedByEmployeeName = r.RequestedByEmployee?.FullName,
        ApprovedByManagerId = r.ApprovedByManagerId, ApprovedByManagerName = r.ApprovedByManager?.FullName,
        Status = r.Status, StatusText = StatusText(r.Status),
        Reason = r.Reason, PhotoPath = r.PhotoPath, Notes = r.Notes,
        RequestedAt = r.RequestedAt, ApprovedAt = r.ApprovedAt, CompletedAt = r.CompletedAt,
        Details = r.Details.Select(d => new SalesReturnDetailDto
        {
            Id = d.Id, ProductId = d.ProductId, ProductName = d.Product?.Name,
            Quantity = d.Quantity, Notes = d.Notes
        }).ToList()
    };

    internal static string StatusText(SalesReturnStatus s) => s switch
    {
        SalesReturnStatus.Pending            => "بانتظار موافقة المدير",
        SalesReturnStatus.ManagerApproved    => "وافق المدير — بانتظار المستودع",
        SalesReturnStatus.WarehouseApproved  => "وافق المستودع — بانتظار المحاسب",
        SalesReturnStatus.AccountantApproved => "وافق المحاسب — جاهز للتنفيذ",
        SalesReturnStatus.Completed          => "مكتملة",
        SalesReturnStatus.Rejected           => "مرفوضة",
        _ => ""
    };
}

// ── Get By Id ─────────────────────────────────────────────────────────────────

public record GetSalesReturnByIdQuery(int Id) : IRequest<SalesReturnDto?>;

public class GetSalesReturnByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetSalesReturnByIdQuery, SalesReturnDto?>
{
    public async Task<SalesReturnDto?> Handle(GetSalesReturnByIdQuery request, CancellationToken ct)
    {
        var r = await uow.SalesReturns.Query()
            .Include(x => x.Invoice)
            .Include(x => x.RequestedByEmployee)
            .Include(x => x.ApprovedByManager)
            .Include(x => x.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        return r is null ? null : GetAllSalesReturnsQueryHandler.Map(r);
    }
}

// ── Create ────────────────────────────────────────────────────────────────────

public record CreateSalesReturnCommand(CreateSalesReturnDto Dto, int EmployeeId) : IRequest<SalesReturnDto>;

public class CreateSalesReturnCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateSalesReturnCommand, SalesReturnDto>
{
    public async Task<SalesReturnDto> Handle(CreateSalesReturnCommand request, CancellationToken ct)
    {
        var ret = new SalesReturn
        {
            InvoiceId = request.Dto.InvoiceId,
            RequestedByEmployeeId = request.EmployeeId,
            Reason = request.Dto.Reason,
            Notes = request.Dto.Notes,
            Status = SalesReturnStatus.Pending,
            Details = request.Dto.Items.Select(i => new SalesReturnDetail
            {
                ProductId = i.ProductId, Quantity = i.Quantity, Notes = i.Notes
            }).ToList()
        };

        await uow.SalesReturns.AddAsync(ret);
        await uow.Notifications.AddAsync(new Notification
        {
            Title = "طلب مرتجع جديد",
            Body = $"تم إرسال طلب مرتجع بانتظار الموافقة: {request.Dto.Reason}",
            Target = NotificationTarget.SalesManager
        });
        await uow.SaveChangesAsync(ct);
        return GetAllSalesReturnsQueryHandler.Map(ret);
    }
}

// ── Manager Approve ───────────────────────────────────────────────────────────

public record ApproveSalesReturnByManagerCommand(int ReturnId, int ManagerId) : IRequest<bool>;

public class ApproveSalesReturnByManagerCommandHandler(IUnitOfWork uow)
    : IRequestHandler<ApproveSalesReturnByManagerCommand, bool>
{
    public async Task<bool> Handle(ApproveSalesReturnByManagerCommand request, CancellationToken ct)
    {
        var ret = await uow.SalesReturns.GetByIdAsync(request.ReturnId);
        if (ret is null || ret.Status != SalesReturnStatus.Pending) return false;
        ret.Status = SalesReturnStatus.ManagerApproved;
        ret.ApprovedByManagerId = request.ManagerId;
        ret.ApprovedAt = DateTime.UtcNow;
        await uow.Notifications.AddAsync(new Notification
        {
            Title = "موافقة المدير على المرتجع",
            Body = "وافق المدير على طلب المرتجع — بانتظار موافقة أمين المستودع",
            Target = NotificationTarget.Employee,
            TargetUserId = ret.RequestedByEmployeeId
        });
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

// ── Warehouse Approve ─────────────────────────────────────────────────────────

public record ApproveSalesReturnByWarehouseCommand(int ReturnId) : IRequest<bool>;

public class ApproveSalesReturnByWarehouseCommandHandler(IUnitOfWork uow)
    : IRequestHandler<ApproveSalesReturnByWarehouseCommand, bool>
{
    public async Task<bool> Handle(ApproveSalesReturnByWarehouseCommand request, CancellationToken ct)
    {
        var ret = await uow.SalesReturns.GetByIdAsync(request.ReturnId);
        if (ret is null || ret.Status != SalesReturnStatus.ManagerApproved) return false;
        ret.Status = SalesReturnStatus.WarehouseApproved;
        await uow.Notifications.AddAsync(new Notification
        {
            Title = "موافقة المستودع على المرتجع",
            Body = "وافق أمين المستودع — بانتظار موافقة المحاسب",
            Target = NotificationTarget.Employee,
            TargetUserId = ret.RequestedByEmployeeId
        });
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

// ── Accountant Approve + Complete ─────────────────────────────────────────────

public record CompleteSalesReturnCommand(int ReturnId) : IRequest<bool>;

public class CompleteSalesReturnCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CompleteSalesReturnCommand, bool>
{
    public async Task<bool> Handle(CompleteSalesReturnCommand request, CancellationToken ct)
    {
        var ret = await uow.SalesReturns.Query()
            .Include(r => r.Details)
            .FirstOrDefaultAsync(r => r.Id == request.ReturnId, ct);
        if (ret is null || ret.Status != SalesReturnStatus.WarehouseApproved) return false;

        ret.Status = SalesReturnStatus.Completed;
        ret.CompletedAt = DateTime.UtcNow;

        // Return stock to inventory (first warehouse — simplified)
        foreach (var detail in ret.Details)
        {
            var inv = await uow.Inventories.Query()
                .FirstOrDefaultAsync(i => i.ProductId == detail.ProductId, ct);
            if (inv != null) inv.Quantity += detail.Quantity;
        }

        await uow.Notifications.AddAsync(new Notification
        {
            Title = "اكتمل طلب المرتجع",
            Body = "تمت الموافقة على المرتجع وتحديث المخزون",
            Target = NotificationTarget.Employee,
            TargetUserId = ret.RequestedByEmployeeId
        });
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

// ── Reject ────────────────────────────────────────────────────────────────────

public record RejectSalesReturnCommand(int ReturnId, string? Reason) : IRequest<bool>;

public class RejectSalesReturnCommandHandler(IUnitOfWork uow)
    : IRequestHandler<RejectSalesReturnCommand, bool>
{
    public async Task<bool> Handle(RejectSalesReturnCommand request, CancellationToken ct)
    {
        var ret = await uow.SalesReturns.GetByIdAsync(request.ReturnId);
        if (ret is null) return false;
        ret.Status = SalesReturnStatus.Rejected;
        if (!string.IsNullOrEmpty(request.Reason)) ret.Notes = request.Reason;
        await uow.Notifications.AddAsync(new Notification
        {
            Title = "تم رفض طلب المرتجع",
            Body = request.Reason ?? "تم رفض طلب المرتجع",
            Target = NotificationTarget.Employee,
            TargetUserId = ret.RequestedByEmployeeId
        });
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
