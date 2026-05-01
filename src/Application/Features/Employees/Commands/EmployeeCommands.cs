using DeliverySystem.Application.DTOs;
using DeliverySystem.Application.Features.Auth.Commands;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Employees.Commands;

// ─── Create Employee ──────────────────────────────────────────────────────────

public record CreateEmployeeCommand(CreateEmployeeDto Dto) : IRequest<EmployeeDto>;

public class CreateEmployeeCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var emp = new Employee
        {
            FullName     = request.Dto.FullName,
            Phone        = request.Dto.Phone,
            Address      = request.Dto.Address,
            Username     = request.Dto.Username,
            PasswordHash = PasswordHelper.Hash(request.Dto.Password),
            IsActive     = request.Dto.IsActive,
            EmployeeType = request.Dto.EmployeeType,
            Roles        = request.Dto.SelectedRoles.Count > 0 ? string.Join(",", request.Dto.SelectedRoles) : null,
            AssignedAreas = request.Dto.SelectedAreas.Count > 0 ? string.Join(",", request.Dto.SelectedAreas) : null,
            CarNumber    = request.Dto.CarNumber,
            CarType      = request.Dto.CarType,
            Region       = request.Dto.Region,
            Branch       = request.Dto.Branch,
            IdImagePath  = request.Dto.IdImagePath,
            PhotoPath    = request.Dto.PhotoPath
        };
        await uow.Employees.AddAsync(emp);
        await uow.SaveChangesAsync(cancellationToken);
        return EmployeeMapper.Map(emp);
    }
}

// ─── Update Employee ──────────────────────────────────────────────────────────

public record UpdateEmployeeCommand(int Id, UpdateEmployeeDto Dto) : IRequest<bool>;

public class UpdateEmployeeCommandHandler(IUnitOfWork uow) : IRequestHandler<UpdateEmployeeCommand, bool>
{
    public async Task<bool> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var emp = await uow.Employees.GetByIdAsync(request.Id);
        if (emp is null) return false;
        emp.FullName     = request.Dto.FullName;
        emp.Phone        = request.Dto.Phone;
        emp.Address      = request.Dto.Address;
        emp.IsActive     = request.Dto.IsActive;
        emp.EmployeeType = request.Dto.EmployeeType;
        emp.Roles        = request.Dto.SelectedRoles.Count > 0 ? string.Join(",", request.Dto.SelectedRoles) : null;
        emp.AssignedAreas = request.Dto.SelectedAreas.Count > 0 ? string.Join(",", request.Dto.SelectedAreas) : null;
        emp.CarNumber    = request.Dto.CarNumber;
        emp.CarType      = request.Dto.CarType;
        emp.Region       = request.Dto.Region;
        emp.Branch       = request.Dto.Branch;
        emp.IdImagePath  = request.Dto.IdImagePath;
        emp.PhotoPath    = request.Dto.PhotoPath;
        uow.Employees.Update(emp);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ─── Delete Employee ──────────────────────────────────────────────────────────

public record DeleteEmployeeCommand(int Id) : IRequest<bool>;

public class DeleteEmployeeCommandHandler(IUnitOfWork uow) : IRequestHandler<DeleteEmployeeCommand, bool>
{
    public async Task<bool> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var emp = await uow.Employees.GetByIdAsync(request.Id);
        if (emp is null) return false;
        uow.Employees.Remove(emp);
        await uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ─── Get All Employees ────────────────────────────────────────────────────────

public record GetAllEmployeesQuery(string? Search = null, string? EmployeeType = null) : IRequest<IEnumerable<EmployeeDto>>;

public class GetAllEmployeesQueryHandler(IUnitOfWork uow) : IRequestHandler<GetAllEmployeesQuery, IEnumerable<EmployeeDto>>
{
    public async Task<IEnumerable<EmployeeDto>> Handle(GetAllEmployeesQuery request, CancellationToken cancellationToken)
    {
        var query = uow.Employees.Query()
            .Include(e => e.Customers).ThenInclude(c => c.Invoices)
            .Include(e => e.Invoices)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(e => e.FullName.Contains(request.Search) || e.Phone.Contains(request.Search));

        if (!string.IsNullOrWhiteSpace(request.EmployeeType))
            query = query.Where(e => e.EmployeeType == request.EmployeeType);

        var employees = await query.ToListAsync(cancellationToken);
        return employees.Select(EmployeeMapper.Map);
    }
}

// ─── Get Employee By Id ───────────────────────────────────────────────────────

public record GetEmployeeByIdQuery(int Id) : IRequest<EmployeeDto?>;

public class GetEmployeeByIdQueryHandler(IUnitOfWork uow) : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto?>
{
    public async Task<EmployeeDto?> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var emp = await uow.Employees.Query()
            .Include(e => e.Customers).ThenInclude(c => c.Invoices)
            .Include(e => e.Invoices)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        return emp is null ? null : EmployeeMapper.Map(emp);
    }
}

// ─── Mapper ───────────────────────────────────────────────────────────────────

public static class EmployeeMapper
{
    public static EmployeeDto Map(Employee e) => new()
    {
        Id = e.Id, FullName = e.FullName, Phone = e.Phone, Address = e.Address,
        Username = e.Username, IsActive = e.IsActive, CreatedAt = e.CreatedAt,
        EmployeeType = e.EmployeeType, Roles = e.Roles, AssignedAreas = e.AssignedAreas,
        CarNumber = e.CarNumber, CarType = e.CarType, Region = e.Region, Branch = e.Branch,
        IdImagePath = e.IdImagePath, PhotoPath = e.PhotoPath,
        CustomerCount = e.Customers.Count,
        TotalCollected = e.Customers.SelectMany(c => c.Invoices).Sum(i => i.PaidAmount),
        TotalDebt = e.Customers.SelectMany(c => c.Invoices).Sum(i => i.TotalAmount - i.PaidAmount),
        ActiveDeliveries = e.Invoices.Count(i => i.Status == Domain.Enums.InvoiceStatus.AwaitingDelivery),
        CompletedDeliveries = e.Invoices.Count(i => i.Status == Domain.Enums.InvoiceStatus.Completed)
    };
}
