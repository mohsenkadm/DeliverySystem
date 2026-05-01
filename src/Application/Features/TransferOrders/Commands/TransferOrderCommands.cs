using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Enums;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.TransferOrders.Commands;

// ── Mapper ────────────────────────────────────────────────────────────────────

public static class TransferOrderMapper
{
    public static TransferOrderDto Map(TransferOrder t) => new()
    {
        Id = t.Id, OrderNumber = t.OrderNumber,
        FromWarehouseId = t.FromWarehouseId, FromWarehouseName = t.FromWarehouse?.Name,
        ToWarehouseId = t.ToWarehouseId, ToWarehouseName = t.ToWarehouse?.Name,
        RequestedByEmployeeId = t.RequestedByEmployeeId, RequestedByEmployeeName = t.RequestedByEmployee?.FullName,
        ApprovedByEmployeeId = t.ApprovedByEmployeeId, ApprovedByEmployeeName = t.ApprovedByEmployee?.FullName,
        Status = t.Status, OrderType = t.OrderType, Notes = t.Notes,
        RequestedAt = t.RequestedAt, ApprovedAt = t.ApprovedAt, CompletedAt = t.CompletedAt,
        Details = t.Details.Select(d => new TransferOrderDetailDto
        {
            Id = d.Id, ProductId = d.ProductId, ProductName = d.Product?.Name,
            RequestedQuantity = d.RequestedQuantity, ApprovedQuantity = d.ApprovedQuantity
        }).ToList()
    };
}

// ── Queries ───────────────────────────────────────────────────────────────────

public record GetAllTransferOrdersQuery(
    int? RequestedByEmployeeId = null,
    TransferOrderStatus? Status = null,
    TransferOrderType? OrderType = null) : IRequest<IEnumerable<TransferOrderDto>>;

public class GetAllTransferOrdersQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetAllTransferOrdersQuery, IEnumerable<TransferOrderDto>>
{
    public async Task<IEnumerable<TransferOrderDto>> Handle(GetAllTransferOrdersQuery request, CancellationToken ct)
    {
        IQueryable<TransferOrder> q = uow.TransferOrders.Query()
            .Include(t => t.FromWarehouse).Include(t => t.ToWarehouse)
            .Include(t => t.RequestedByEmployee).Include(t => t.ApprovedByEmployee)
            .Include(t => t.Details).ThenInclude(d => d.Product);

        if (request.RequestedByEmployeeId.HasValue)
            q = q.Where(t => t.RequestedByEmployeeId == request.RequestedByEmployeeId.Value);
        if (request.Status.HasValue)
            q = q.Where(t => t.Status == request.Status.Value);
        if (request.OrderType.HasValue)
            q = q.Where(t => t.OrderType == request.OrderType.Value);

        var list = await q.OrderByDescending(t => t.RequestedAt).ToListAsync(ct);
        return list.Select(TransferOrderMapper.Map);
    }
}

public record GetTransferOrderByIdQuery(int Id) : IRequest<TransferOrderDto?>;

public class GetTransferOrderByIdQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetTransferOrderByIdQuery, TransferOrderDto?>
{
    public async Task<TransferOrderDto?> Handle(GetTransferOrderByIdQuery request, CancellationToken ct)
    {
        var t = await uow.TransferOrders.Query()
            .Include(x => x.FromWarehouse).Include(x => x.ToWarehouse)
            .Include(x => x.RequestedByEmployee).Include(x => x.ApprovedByEmployee)
            .Include(x => x.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        return t is null ? null : TransferOrderMapper.Map(t);
    }
}

// ── Commands ──────────────────────────────────────────────────────────────────

public record CreateTransferOrderCommand(CreateTransferOrderDto Dto, int RequestedByEmployeeId) : IRequest<TransferOrderDto>;

public class CreateTransferOrderCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateTransferOrderCommand, TransferOrderDto>
{
    public async Task<TransferOrderDto> Handle(CreateTransferOrderCommand request, CancellationToken ct)
    {
        var order = new TransferOrder
        {
            OrderNumber = $"TR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            FromWarehouseId = request.Dto.FromWarehouseId,
            ToWarehouseId = request.Dto.ToWarehouseId,
            RequestedByEmployeeId = request.RequestedByEmployeeId,
            OrderType = request.Dto.OrderType,
            Notes = request.Dto.Notes,
            Status = TransferOrderStatus.Pending,
            Details = request.Dto.Details.Select(d => new TransferOrderDetail
            {
                ProductId = d.ProductId,
                RequestedQuantity = d.RequestedQuantity
            }).ToList()
        };
        await uow.TransferOrders.AddAsync(order);
        await uow.SaveChangesAsync(ct);
        return TransferOrderMapper.Map(order);
    }
}

/// Accountant approves transfer order
public record ApproveTransferOrderCommand(int Id, ApproveTransferOrderDto Dto) : IRequest<bool>;

public class ApproveTransferOrderCommandHandler(IUnitOfWork uow)
    : IRequestHandler<ApproveTransferOrderCommand, bool>
{
    public async Task<bool> Handle(ApproveTransferOrderCommand request, CancellationToken ct)
    {
        var order = await uow.TransferOrders.Query()
            .Include(t => t.Details)
            .FirstOrDefaultAsync(t => t.Id == request.Id, ct);
        if (order is null || order.Status != TransferOrderStatus.Pending) return false;

        order.Status = TransferOrderStatus.AccountantApproved;
        order.ApprovedByEmployeeId = request.Dto.ApprovedByEmployeeId;
        order.ApprovedAt = DateTime.UtcNow;

        if (request.Dto.Details is not null)
        {
            foreach (var ad in request.Dto.Details)
            {
                var detail = order.Details.FirstOrDefault(d => d.Id == ad.DetailId);
                if (detail is not null) detail.ApprovedQuantity = ad.ApprovedQuantity;
            }
        }
        else
        {
            foreach (var d in order.Details)
                d.ApprovedQuantity ??= d.RequestedQuantity;
        }

        await uow.SaveChangesAsync(ct);
        return true;
    }
}

/// Warehouse manager starts processing
public record StartWarehouseProcessingCommand(int Id) : IRequest<bool>;

public class StartWarehouseProcessingCommandHandler(IUnitOfWork uow)
    : IRequestHandler<StartWarehouseProcessingCommand, bool>
{
    public async Task<bool> Handle(StartWarehouseProcessingCommand request, CancellationToken ct)
    {
        var order = await uow.TransferOrders.GetByIdAsync(request.Id);
        if (order is null || order.Status != TransferOrderStatus.AccountantApproved) return false;
        order.Status = TransferOrderStatus.WarehouseProcessing;
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

/// Warehouse manager completes transfer — updates inventory
public record CompleteTransferOrderCommand(int Id) : IRequest<bool>;

public class CompleteTransferOrderCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CompleteTransferOrderCommand, bool>
{
    public async Task<bool> Handle(CompleteTransferOrderCommand request, CancellationToken ct)
    {
        var order = await uow.TransferOrders.Query()
            .Include(t => t.Details)
            .FirstOrDefaultAsync(t => t.Id == request.Id, ct);
        if (order is null || order.Status != TransferOrderStatus.WarehouseProcessing) return false;

        foreach (var detail in order.Details)
        {
            var qty = detail.ApprovedQuantity ?? detail.RequestedQuantity;

            // Deduct from source warehouse
            var fromInv = await uow.Inventories.Query()
                .FirstOrDefaultAsync(i => i.WarehouseId == order.FromWarehouseId && i.ProductId == detail.ProductId, ct);
            if (fromInv is not null)
                fromInv.Quantity = Math.Max(0, fromInv.Quantity - qty);

            // Add to destination warehouse
            var toInv = await uow.Inventories.Query()
                .FirstOrDefaultAsync(i => i.WarehouseId == order.ToWarehouseId && i.ProductId == detail.ProductId, ct);
            if (toInv is null)
            {
                toInv = new Inventory { WarehouseId = order.ToWarehouseId, ProductId = detail.ProductId, Quantity = 0 };
                await uow.Inventories.AddAsync(toInv);
            }
            toInv.Quantity += qty;
        }

        order.Status = TransferOrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

/// Reject transfer order
public record RejectTransferOrderCommand(int Id) : IRequest<bool>;

public class RejectTransferOrderCommandHandler(IUnitOfWork uow)
    : IRequestHandler<RejectTransferOrderCommand, bool>
{
    public async Task<bool> Handle(RejectTransferOrderCommand request, CancellationToken ct)
    {
        var order = await uow.TransferOrders.GetByIdAsync(request.Id);
        if (order is null) return false;
        order.Status = TransferOrderStatus.Rejected;
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
