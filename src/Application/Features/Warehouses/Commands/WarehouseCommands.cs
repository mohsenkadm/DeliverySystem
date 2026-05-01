using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Warehouses.Commands;

/// <summary>أمر إنشاء مستودع جديد</summary>
public record CreateWarehouseCommand(CreateWarehouseDto Dto) : IRequest<WarehouseDto>;

/// <summary>معالج أمر إنشاء المستودع</summary>
public class CreateWarehouseCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var wh = new Warehouse { Name = request.Dto.Name, Location = request.Dto.Location };
        await uow.Warehouses.AddAsync(wh);
        await uow.SaveChangesAsync(cancellationToken);
        return new WarehouseDto { Id = wh.Id, Name = wh.Name, Location = wh.Location, CreatedAt = wh.CreatedAt };
    }
}

/// <summary>أمر تعديل مستودع</summary>
public record UpdateWarehouseCommand(int Id, CreateWarehouseDto Dto) : IRequest<bool>;

/// <summary>معالج أمر تعديل المستودع</summary>
public class UpdateWarehouseCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateWarehouseCommand, bool>
{
    public async Task<bool> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var wh = await uow.Warehouses.GetByIdAsync(request.Id);
        if (wh is null) return false;
        wh.Name = request.Dto.Name; wh.Location = request.Dto.Location;
        uow.Warehouses.Update(wh);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>أمر حذف مستودع</summary>
public record DeleteWarehouseCommand(int Id) : IRequest<bool>;

/// <summary>معالج أمر حذف المستودع</summary>
public class DeleteWarehouseCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteWarehouseCommand, bool>
{
    public async Task<bool> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var wh = await uow.Warehouses.GetByIdAsync(request.Id);
        if (wh is null) return false;
        uow.Warehouses.Remove(wh);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>استعلام جلب جميع المستودعات</summary>
public record GetAllWarehousesQuery(string? Search = null) : IRequest<IEnumerable<WarehouseDto>>;

/// <summary>معالج استعلام جلب المستودعات</summary>
public class GetAllWarehousesQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAllWarehousesQuery, IEnumerable<WarehouseDto>>
{
    public async Task<IEnumerable<WarehouseDto>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        var query = uow.Warehouses.Query().Include(w => w.Inventories).AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(w => w.Name.Contains(request.Search));
        var list = await query.ToListAsync(cancellationToken);
        return list.Select(w => new WarehouseDto
        {
            Id = w.Id, Name = w.Name, Location = w.Location,
            ProductCount = w.Inventories.Select(i => i.ProductId).Distinct().Count(),
            CreatedAt = w.CreatedAt
        });
    }
}

/// <summary>استعلام جلب مستودع بالمعرف</summary>
public record GetWarehouseByIdQuery(int Id) : IRequest<WarehouseDto?>;

/// <summary>معالج استعلام جلب مستودع بالمعرف</summary>
public class GetWarehouseByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto?>
{
    public async Task<WarehouseDto?> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var w = await uow.Warehouses.Query().Include(x => x.Inventories)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (w is null) return null;
        return new WarehouseDto
        {
            Id = w.Id, Name = w.Name, Location = w.Location,
            ProductCount = w.Inventories.Select(i => i.ProductId).Distinct().Count(),
            CreatedAt = w.CreatedAt
        };
    }
}
