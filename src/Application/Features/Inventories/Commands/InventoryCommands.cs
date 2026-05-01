using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Inventories.Commands;

/// <summary>أمر إضافة/تحديث مخزون منتج في مستودع</summary>
public record UpsertInventoryCommand(CreateInventoryDto Dto) : IRequest<InventoryDto>;

/// <summary>معالج أمر إضافة/تحديث المخزون</summary>
public class UpsertInventoryCommandHandler(IUnitOfWork uow) : IRequestHandler<UpsertInventoryCommand, InventoryDto>
{
    public async Task<InventoryDto> Handle(UpsertInventoryCommand request, CancellationToken cancellationToken)
    {
        var existing = await uow.Inventories.FirstOrDefaultAsync(
            i => i.ProductId == request.Dto.ProductId && i.WarehouseId == request.Dto.WarehouseId);
        if (existing is not null)
        {
            existing.Quantity += request.Dto.Quantity;
            existing.Date = DateTime.UtcNow;
            uow.Inventories.Update(existing);
            await uow.SaveChangesAsync(cancellationToken);
            return new InventoryDto { Id = existing.Id, ProductId = existing.ProductId, WarehouseId = existing.WarehouseId, Quantity = existing.Quantity, Date = existing.Date };
        }
        var inventory = new Inventory
        {
            ProductId = request.Dto.ProductId,
            WarehouseId = request.Dto.WarehouseId,
            Quantity = request.Dto.Quantity,
            Date = DateTime.UtcNow
        };
        await uow.Inventories.AddAsync(inventory);
        await uow.SaveChangesAsync(cancellationToken);
        return new InventoryDto { Id = inventory.Id, ProductId = inventory.ProductId, WarehouseId = inventory.WarehouseId, Quantity = inventory.Quantity, Date = inventory.Date };
    }
}

/// <summary>أمر تعديل كمية المخزون</summary>
public record UpdateInventoryCommand(int Id, int Quantity) : IRequest<bool>;

/// <summary>معالج أمر تعديل المخزون</summary>
public class UpdateInventoryCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateInventoryCommand, bool>
{
    public async Task<bool> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        var inv = await uow.Inventories.GetByIdAsync(request.Id);
        if (inv is null) return false;
        inv.Quantity = request.Quantity; inv.Date = DateTime.UtcNow;
        uow.Inventories.Update(inv);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>أمر حذف مخزون</summary>
public record DeleteInventoryCommand(int Id) : IRequest<bool>;

/// <summary>معالج أمر حذف المخزون</summary>
public class DeleteInventoryCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteInventoryCommand, bool>
{
    public async Task<bool> Handle(DeleteInventoryCommand request, CancellationToken cancellationToken)
    {
        var inv = await uow.Inventories.GetByIdAsync(request.Id);
        if (inv is null) return false;
        uow.Inventories.Remove(inv);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>استعلام جلب جميع المخزون</summary>
public record GetAllInventoriesQuery(int? ProductId = null, int? WarehouseId = null) : IRequest<IEnumerable<InventoryDto>>;

/// <summary>معالج استعلام جلب المخزون</summary>
public class GetAllInventoriesQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAllInventoriesQuery, IEnumerable<InventoryDto>>
{
    public async Task<IEnumerable<InventoryDto>> Handle(GetAllInventoriesQuery request, CancellationToken cancellationToken)
    {
        var query = uow.Inventories.Query()
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .AsQueryable();
        if (request.ProductId.HasValue) query = query.Where(i => i.ProductId == request.ProductId);
        if (request.WarehouseId.HasValue) query = query.Where(i => i.WarehouseId == request.WarehouseId);
        var list = await query.ToListAsync(cancellationToken);
        return list.Select(i => new InventoryDto
        {
            Id = i.Id, ProductId = i.ProductId, ProductName = i.Product?.Name,
            WarehouseId = i.WarehouseId, WarehouseName = i.Warehouse?.Name,
            Quantity = i.Quantity, Date = i.Date
        });
    }
}
