using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Branches.Commands;

// ── Queries ─────────────────────────────────────────────────────────────────

public record GetAllBranchesQuery(string? Search = null, bool? IsActive = null) : IRequest<IEnumerable<BranchDto>>;

public class GetAllBranchesQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAllBranchesQuery, IEnumerable<BranchDto>>
{
    public async Task<IEnumerable<BranchDto>> Handle(GetAllBranchesQuery request, CancellationToken ct)
    {
        var q = uow.Branches.Query();
        if (!string.IsNullOrWhiteSpace(request.Search))
            q = q.Where(b => b.Name.Contains(request.Search) || (b.Region != null && b.Region.Contains(request.Search)));
        if (request.IsActive.HasValue)
            q = q.Where(b => b.IsActive == request.IsActive.Value);
        return await q.Select(b => new BranchDto
        {
            Id = b.Id, Name = b.Name, Address = b.Address, Phone = b.Phone,
            Region = b.Region, IsActive = b.IsActive, CreatedAt = b.CreatedAt,
            CustomerCount  = b.Customers.Count,
            EmployeeCount  = b.Employees.Count,
            WarehouseCount = b.Warehouses.Count
        }).ToListAsync(ct);
    }
}

public record GetBranchByIdQuery(int Id) : IRequest<BranchDto?>;

public class GetBranchByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetBranchByIdQuery, BranchDto?>
{
    public async Task<BranchDto?> Handle(GetBranchByIdQuery request, CancellationToken ct)
        => await uow.Branches.Query().Where(b => b.Id == request.Id).Select(b => new BranchDto
        {
            Id = b.Id, Name = b.Name, Address = b.Address, Phone = b.Phone,
            Region = b.Region, IsActive = b.IsActive, CreatedAt = b.CreatedAt,
            CustomerCount  = b.Customers.Count,
            EmployeeCount  = b.Employees.Count,
            WarehouseCount = b.Warehouses.Count
        }).FirstOrDefaultAsync(ct);
}

// ── Commands ─────────────────────────────────────────────────────────────────

public record CreateBranchCommand(CreateBranchDto Dto) : IRequest<BranchDto>;

public class CreateBranchCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateBranchCommand, BranchDto>
{
    public async Task<BranchDto> Handle(CreateBranchCommand request, CancellationToken ct)
    {
        var b = new Branch
        {
            Name    = request.Dto.Name,
            Address = request.Dto.Address,
            Phone   = request.Dto.Phone,
            Region  = request.Dto.Region,
            IsActive = true
        };
        await uow.Branches.AddAsync(b);
        await uow.SaveChangesAsync(ct);
        return new BranchDto { Id = b.Id, Name = b.Name, Address = b.Address, Phone = b.Phone, Region = b.Region, IsActive = b.IsActive, CreatedAt = b.CreatedAt };
    }
}

public record UpdateBranchCommand(int Id, UpdateBranchDto Dto) : IRequest<bool>;

public class UpdateBranchCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateBranchCommand, bool>
{
    public async Task<bool> Handle(UpdateBranchCommand request, CancellationToken ct)
    {
        var b = await uow.Branches.GetByIdAsync(request.Id);
        if (b is null) return false;
        b.Name    = request.Dto.Name;
        b.Address = request.Dto.Address;
        b.Phone   = request.Dto.Phone;
        b.Region  = request.Dto.Region;
        b.IsActive = request.Dto.IsActive;
        await uow.SaveChangesAsync(ct);
        return true;
    }
}

public record DeleteBranchCommand(int Id) : IRequest<bool>;

public class DeleteBranchCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteBranchCommand, bool>
{
    public async Task<bool> Handle(DeleteBranchCommand request, CancellationToken ct)
    {
        var b = await uow.Branches.GetByIdAsync(request.Id);
        if (b is null) return false;
        uow.Branches.Remove(b);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
