using DeliverySystem.Application.DTOs;
using DeliverySystem.Domain.Entities;
using DeliverySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Application.Features.Employees.Commands;

public record GetEmployeePermissionsQuery(int EmployeeId) : IRequest<List<EmployeePermissionDto>>;

public class GetEmployeePermissionsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetEmployeePermissionsQuery, List<EmployeePermissionDto>>
{
    public async Task<List<EmployeePermissionDto>> Handle(GetEmployeePermissionsQuery request, CancellationToken ct)
        => await uow.EmployeePermissions.Query()
            .Where(p => p.EmployeeId == request.EmployeeId)
            .Select(p => new EmployeePermissionDto
            {
                Id = p.Id, PageName = p.PageName,
                CanView = p.CanView, CanAdd = p.CanAdd, CanEdit = p.CanEdit, CanDelete = p.CanDelete
            })
            .ToListAsync(ct);
}

public record SaveEmployeePermissionsCommand(int EmployeeId, List<EmployeePermissionDto> Permissions) : IRequest;

public class SaveEmployeePermissionsCommandHandler(IUnitOfWork uow)
    : IRequestHandler<SaveEmployeePermissionsCommand>
{
    public async Task Handle(SaveEmployeePermissionsCommand request, CancellationToken ct)
    {
        var existing = await uow.EmployeePermissions.Query()
            .Where(p => p.EmployeeId == request.EmployeeId).ToListAsync(ct);
        uow.EmployeePermissions.RemoveRange(existing);

        foreach (var dto in request.Permissions)
        {
            await uow.EmployeePermissions.AddAsync(new EmployeePermission
            {
                EmployeeId = request.EmployeeId,
                PageName = dto.PageName,
                CanView = dto.CanView, CanAdd = dto.CanAdd,
                CanEdit = dto.CanEdit, CanDelete = dto.CanDelete
            });
        }
        await uow.SaveChangesAsync(ct);
    }
}
